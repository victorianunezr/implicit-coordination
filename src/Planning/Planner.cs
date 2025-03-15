using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class Planner
    {
        private const string NoSolutionMessage = "All root worlds have cost infinity. Game over :(";
        private readonly Domain Domain;
        private readonly Problem Problem;
        private bool NewWorldsPruned = true;
        public ICollection<State> Leaves = new List<State>();
        public State Root;
        public IEnumerable<World> WorldsInRoot => Root.possibleWorlds.OfType<World>();
        public Queue<State> Frontier = new();

        public Planner(Domain domain, Problem problem)
        {
            Domain = domain;
            Problem = problem;
            Root = problem.initialState;
            State.Problem = Problem;
            World.Problem = Problem;
        }
    
        public void Plan()
        {
            try
            {
                DebugLogger.IsEnabled = true;

                Frontier.Enqueue(Root);

                while (!SolutionFound())
                {
                    if (NotSolvable())
                    {
                        Console.WriteLine(NoSolutionMessage);
                        return;
                    }

                    // Step 1
                    Console.WriteLine("Step 1: Build tree.");
                    BuildTree();

                    // Step 2
                    Console.WriteLine("Step 2: Computing objective costs.");
                    ComputeObjectiveCosts();

                    // Step 3
                    // if any root world has cost +, iterate on depth
                    while (UndefinedCostsInRoot())
                    {
                        if (NotSolvable())
                        {
                            Console.WriteLine(NoSolutionMessage);
                            return;
                        }

                        Console.WriteLine("Some root world has objective cost '+'");

                        Leaves.Clear();

                        Console.WriteLine("Iterating on cutoff depth. Expanding tree further.");

                        BuildTree();

                        Console.WriteLine("Recomputing objective costs...");
                        ComputeObjectiveCosts();
                    }

                    // Step 4
                    Console.WriteLine("Step 4: Cutting indistinguishability edges.");
                    CutIndistinguishabilityEdges();

                    while (NewWorldsPruned)
                    {
                        // Step 5
                        Console.WriteLine("Step 5: Computing subjective costs.");
                        ComputeSubjectiveCosts();
                        
                        // Step 6
                        Console.WriteLine("Step 6: Pruning tree");
                        Prune();
                    }
                }
            }
            catch {
                Console.WriteLine("yep im here");
            }
            finally
            {
                // Print tree
                TreeVisualizer.ExportWorldGraphAsDot(Root, "/Users/victorianunezr/repos/implicit-coordination/epddl/output/goalrecognition-lever.dot");
                TreeVisualizer.PrintStateAsYaml(Root, "/Users/victorianunezr/repos/implicit-coordination/epddl/output/goalrecognition-lever.yaml");
            }
        }

        private bool UndefinedCostsInRoot()
        {
            return WorldsInRoot.Any(w => w.objectiveCost.Type == CostType.Undefined);
        }

        private bool SolutionFound()
        {
            return WorldsInRoot.All(w => w.subjectiveCost.Type == CostType.Finite);
        }

        private bool NotSolvable()
        {
            return WorldsInRoot.All(w => w.objectiveCost.Type == CostType.Infinity) ||
                   WorldsInRoot.All(w => w.subjectiveCost.Type == CostType.Infinity);
        }

        private void BuildTree()
        {
            int cutoffDepth = int.MaxValue;

            State s;

            while (Frontier.Any())
            {
                if (Frontier.Peek().depth >= cutoffDepth)
                {
                    // stop expanding if we passed the cutoff depth
                    break;
                }

                s = Frontier.Dequeue();
                foreach (Action action in Domain.actions)
                {
                    if (s.IsApplicable(action))
                    {
                        State sPrime = s.ProductUpdate(action);
                        s.Children.Add(sPrime);

                        if (cutoffDepth == int.MaxValue)
                        {
                            if (sPrime.HasGoalWorld(Problem.goalFormula))
                            {
                                cutoffDepth = sPrime.depth;
                                DebugLogger.Print($"Found goal world at depth {cutoffDepth}.");
                            }
                        }
                        Frontier.Enqueue(sPrime);
                    }
                }
            }

            foreach (State state in Frontier)
            {
                if (state.depth == cutoffDepth)
                {
                    Leaves.Add(state);
                }
                DebugLogger.Print($"Added {Leaves.Count()} leaves at cutoff depth {cutoffDepth}.");
            }
        }

        private void CutIndistinguishabilityEdges()
        {
            var queue = new Queue<State>();
            var visited = new HashSet<State>();

            // We don't cut the edges in root, so start from its children
            foreach (var state in Root.Children)
            {
                queue.Enqueue(state);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited.Add(current);
                
                foreach (var agentEdges in current.accessibility.graph)
                {
                    var edges = agentEdges.Value;

                    foreach (var (world1, world2) in edges)
                    {
                        if (world1 != world2 && world1 is World w1 && world2 is World w2)
                        {
                            if ((w1.objectiveCost.Type == CostType.Finite && (w2.objectiveCost.Type == CostType.Infinity || w2.objectiveCost.Type == CostType.Undefined))
                            || (w2.objectiveCost.Type == CostType.Finite && (w1.objectiveCost.Type == CostType.Infinity || w1.objectiveCost.Type == CostType.Undefined)))
                            {
                                current.accessibility.cutEdges.Add((world1, world2));
                                DebugLogger.Print($"Cutting edge between world {w1.Name} and {w2.Name}.");
                            }
                        }
                    }
                }
                
                foreach (var state in current.Children)
                {
                    if (!visited.Contains(state))
                    {
                        queue.Enqueue(state);
                    }
                }
            }
        }

        private void ComputeObjectiveCosts()
        {
            Action<World, Cost> objectiveAssigner = (w, cost) => 
            {
                DebugLogger.Print($"c_o({w.Name}) = {cost}");
                w.objectiveCost = cost;
            };

            BottomUpCostTraversal(
                ObjectiveCostAggregator,
                objectiveAssigner
            );
        }

        private void ComputeSubjectiveCosts()
        {
            Action<World, Cost> subjectiveAssigner = (w, cost) =>
            {
                DebugLogger.Print($"c_s({w.Name}) = {cost}");
                w.subjectiveCost = cost;
            };  

            BottomUpCostTraversal(
                SubjectiveCostAggregator,
                subjectiveAssigner
            );
        }

        private void ComputeWorldCost(
            State state,
            World w,
            Func<World, State, IEnumerable<Cost>> costAggregator,
            Action<World, Cost> costAssigner)
        {
            if (Problem.goalFormula.Evaluate(state, w))
            {
                costAssigner(w, Cost.Finite(0));
            }
            else if (!w.HasAnyApplicableEvent(Domain.actions, state))
            {
                costAssigner(w, Cost.Infinity());
            }
            else if (Leaves.Contains(state))
            {
                costAssigner(w, Cost.Undefined());
            }
            else
            {
                var costs = costAggregator(w, state).ToList();
                var minActionCost = costs.Min();
                costAssigner(w, minActionCost);
            }
        }

        private IEnumerable<Cost> ObjectiveCostAggregator(World w, State state)
        {
            return w.outgoingEdges
                .Where(edge => !edge.isPruned && !edge.childWorld.isPruned)
                .GroupBy(edge => new { edge.action, edge.actingAgent })
                .Select(group =>
                {
                    var maxChildCost = group.Max(edge => edge.childWorld.objectiveCost);
                    if (maxChildCost.Type == CostType.Finite)
                        return Cost.Finite(maxChildCost.Value + 1);
                    else
                        return maxChildCost;
                });
        }

        private IEnumerable<Cost> SubjectiveCostAggregator(World w, State state)
        {
            return w.outgoingEdges
                .Where(edge => !edge.isPruned && !edge.childWorld.isPruned)
                .GroupBy(edge => new { edge.action, edge.actingAgent })
                .Select(group =>
                {
                    var actingAgent = group.First().actingAgent;
                    var groupAction = group.Key.action;

                    // Costs from accessible worlds' children
                    var accessibleChildCosts = state.accessibility.GetAccessibleWorlds(actingAgent, w, includeCutEdges: false)
                        .OfType<World>()
                        .Where(world => !world.isPruned)
                        .SelectMany(world => world.outgoingEdges)
                        .Where(edge => edge.action.Equals(groupAction) && edge.actingAgent.Equals(actingAgent))
                        .Select(edge => edge.childWorld.subjectiveCost);

                    // Costs from the current world's children
                    var directChildCosts = group.Select(edge => edge.childWorld.subjectiveCost);

                    // Compute the maximum cost from both sources and add 1
                    var maxChildCost = accessibleChildCosts.Concat(directChildCosts).Max();
                    Cost edgeCost;

                    if (maxChildCost.Type == CostType.Finite)
                        edgeCost = Cost.Finite(maxChildCost.Value + 1);
                    else
                        edgeCost = maxChildCost;

                    // Assign the computed cost to each edge in the group
                    foreach (var edge in group)
                    {
                        edge.cost = edgeCost;
                    }

                    // Return the cost for this i:a edge
                    return edgeCost;
                });
        }


        public void BottomUpCostTraversal(
            Func<World, State, IEnumerable<Cost>> costAggregator,
            Action<World, Cost> costAssigner)
        {
            var processedStates = new HashSet<State>();
            // Start with the leaves (states with no children).
            var currentLayer = new HashSet<State>(Leaves);

            while (currentLayer.Any())
            {
                // Process every state in the current layer. Only compute world costs of unpruned worlds.
                foreach (var state in currentLayer)
                {
                    foreach (var world in state.possibleWorlds.OfType<World>().Where(world => !world.isPruned))
                    {
                        ComputeWorldCost(state, world, costAggregator, costAssigner);
                    }
                    processedStates.Add(state);
                }

                // Build the next layer: for each state in the current layer, consider its parent.
                // Only add a parent if all of its children have been processed.
                var nextLayer = new HashSet<State>();
                foreach (var state in currentLayer)
                {
                    if (state.Parent != null && !processedStates.Contains(state.Parent))
                    {
                        if (state.Parent.Children.All(child => processedStates.Contains(child)))
                        {
                            nextLayer.Add(state.Parent);
                        }
                    }
                }
                currentLayer = nextLayer;
            }
        }

        public void Prune()
        {
            NewWorldsPruned = false;

            // Top-down traversal (BFS) starting from root
            var statesToVisit = new Queue<State>();
            statesToVisit.Enqueue(Root);

            while (statesToVisit.Any())
            {
                var state = statesToVisit.Dequeue();

                // Prune worlds if indicated by incoming edges
                if (state.depth > 0)
                {
                    foreach (var w in state.possibleWorlds.OfType<World>().Where(w => !w.isPruned))
                    {
                        if (w.incomingEdge?.isPruned == true)
                        {
                            PruneWorld(w);
                            DebugLogger.Print($"Pruning world {w.Name}");
                            NewWorldsPruned = true;
                        }
                    }
                }

                // Compute costs and prune outgoing edges
                foreach (var w in state.possibleWorlds.OfType<World>().Where(w => !w.isPruned))
                {
                    ComputeWorldAgentCosts(state, w);

                    foreach (var edge in w.outgoingEdges.Where(e => !e.isPruned))
                    {
                        var agent = edge.actingAgent;
                        if (agent == null) throw new Exception($"Action {edge.action.name} has no owner.");

                        // Prune edges whose cost is higher than the world’s cost for that agent
                        if (w.worldAgentCost[agent] < edge.cost)
                        {
                            edge.isPruned = true;
                            DebugLogger.Print($"Pruning edge between {edge.parentWorld.Name} and {edge.childWorld.Name}");
                            NewWorldsPruned = true;
                        }
                    }
                }

                // Enqueue children
                foreach (var child in state.Children)
                {
                    statesToVisit.Enqueue(child);
                }
            }

            if (!NewWorldsPruned) DebugLogger.Print("Nothing to prune.");
        }

        private void PruneWorld(World w)
        {
            w.isPruned = true;
            foreach (var edge in w.outgoingEdges)
            {
                edge.isPruned = true;
            }
        }

        private void ComputeWorldAgentCosts(State state, World w)
        {
            if (w.subjectiveCost.Type == CostType.Unassigned)
            {
                throw new Exception("All worlds must have an assigned cost(w) before computing world-agent costs");
            }

            // todo: restrict agents to action owners
            foreach (Agent agent in state.accessibility.graph.Keys)
            {
                var accessibleUnprunedWorlds = state.accessibility
                    .GetAccessibleWorlds(agent, w)
                    .OfType<World>()
                    .Where(world => !world.isPruned);

                Cost cost = accessibleUnprunedWorlds.Select(w => w.subjectiveCost).Max();
                w.worldAgentCost[agent] = cost;
            }
        }
    }
}
