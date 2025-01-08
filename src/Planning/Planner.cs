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
        }
    
        public void Plan()
        {
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

                    Console.WriteLine("Some root world has cost '+'");

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

                        if (cutoffDepth == int.MaxValue)
                        {
                            if (sPrime.HasGoalWorld(this.Problem.goalFormula))
                            {
                                cutoffDepth = sPrime.depth;
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
            }
        }

        private void CutIndistinguishabilityEdges()
        {
            var queue = new Queue<State>();
            var visited = new HashSet<State>();

            queue.Enqueue(Root);
            visited.Add(Root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                foreach (var agentEdges in current.accessibility.graph)
                {
                    var edges = agentEdges.Value;

                    foreach (var (world1, world2) in edges)
                    {
                        if (world1 is World w1 && world2 is World w2)
                        {
                            if (w1.cost.Type == CostType.Finite && (w2.cost.Type == CostType.Infinity ||w2.cost.Type == CostType.Undefined))
                            {
                                current.accessibility.cutEdges.Add((world1, world2));
                            }
                        }
                    }
                }
                
                foreach (var state in current.Children)
                {
                    if (!visited.Contains(state))
                    {
                        visited.Add(state);
                        queue.Enqueue(state);
                    }
                }
            }
        }

        private void ComputeObjectiveCosts()
        {
            Action<World, Cost> objectiveAssigner = (w, cost) => w.objectiveCost = cost;

            BottomUpCostTraversal(
                ObjectiveCostAggregator,
                objectiveAssigner
            );
        }

        private void ComputeSubjectiveCosts()
        {
            Action<World, Cost> subjectiveAssigner = (w, cost) => w.subjectiveCost = cost;

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

                if (costs.Any())
                {
                    var minActionCost = costs.Min();
                    costAssigner(w, Cost.Finite(minActionCost.Value + 1));
                }
                else
                {
                    costAssigner(w, Cost.Infinity());
                }
            }
        }

        private IEnumerable<Cost> ObjectiveCostAggregator(World w, State state)
        {
            return w.outgoingEdges
                .GroupBy(edge => edge.action)
                .Select(group => group.Max(edge => edge.childWorld.objectiveCost));
        }

        private IEnumerable<Cost> SubjectiveCostAggregator(World w, State state)
        {
            return w.outgoingEdges
                .Where(edge => !edge.isPruned && !edge.childWorld.isPruned)
                .GroupBy(edge => edge.action)
                .Select(group =>
                {
                    var actionOwner = group.First().action.owner;

                    // Costs from accessible worlds' children
                    var accessibleChildCosts = state.accessibility.GetAccessibleWorlds(actionOwner, w, includeCutEdges:false)
                        .OfType<World>()
                        .Where(world => !world.isPruned)
                        .SelectMany(world => world.outgoingEdges)
                        .Select(edge => edge.childWorld.subjectiveCost);

                    // Costs from the current world's children
                    var directChildCosts = group.Select(edge => edge.childWorld.subjectiveCost);

                    // Combine and get the max cost
                    return accessibleChildCosts
                        .Concat(directChildCosts)
                        .Max();
                });
        }

        public void BottomUpCostTraversal(
            Func<World, State, IEnumerable<Cost>> costAggregator,
            Action<World, Cost> costAssigner)
        {
            var visitedStates = new HashSet<State>();
            var stateStack = new Stack<State>();

            foreach (var leaf in Leaves)
            {
                if (!visitedStates.Contains(leaf))
                {
                    stateStack.Push(leaf);
                }
            }

            while (stateStack.Count > 0)
            {
                var currentState = stateStack.Pop();

                if (visitedStates.Contains(currentState))
                    continue;

                foreach (var world in currentState.possibleWorlds.OfType<World>())
                {
                    ComputeWorldCost(
                        currentState, 
                        world, 
                        costAggregator,
                        costAssigner
                    );
                }

                visitedStates.Add(currentState);

                if (currentState.Parent != null && !visitedStates.Contains(currentState.Parent))
                {
                    stateStack.Push(currentState.Parent);
                }
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
                    foreach (var w in state.possibleWorlds.OfType<World>())
                    {
                        if (w.incomingEdge?.isPruned == true)
                        {
                            PruneWorld(w);
                            NewWorldsPruned = true;
                        }
                    }
                }

                // Compute costs and prune outgoing edges
                foreach (var w in state.possibleWorlds.OfType<World>())
                {
                    if (w.isPruned) continue;  // Skip if already pruned

                    ComputeWorldAgentCosts(state, w);

                    foreach (var edge in w.outgoingEdges)
                    {
                        var agent = edge.action.owner;
                        if (agent == null) throw new Exception($"Action {edge.action.name} has no owner."); // Or handle no-agent case

                        // Prune edges whose cost is higher than the world’s cost for that agent
                        if (w.worldAgentCost[agent] < edge.cost)
                        {
                            edge.isPruned = true;
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
            if (w.cost.Type == CostType.Unassigned)
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
                w.worldAgentCost.Add(agent, cost);
            }
        }
    }
}
