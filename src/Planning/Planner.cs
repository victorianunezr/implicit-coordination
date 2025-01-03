using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class Planner
    {
        private readonly Domain Domain;
        private readonly Problem Problem;
        /// <summary>
        /// Maintains a set of the leaf nodes in the graph. Dynamically updated during planning, after node expansion.
        /// </summary>
        public ICollection<State> Leaves = new List<State>();
        public State Root;
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

            // todo: iterate on pruning
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
                var worldsInRoot = Root.possibleWorlds.OfType<World>();
                if (worldsInRoot.All(w => w.objectiveCost.Type == CostType.Infinity))
                {
                    Console.WriteLine("All root worlds have cost infinity. Game over :(");
                    // Problem not solvable. Give up
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

            // Step 5
            Console.WriteLine("Step 5: Computing subjective costs.");
            ComputeSubjectiveCosts();
            
            // Step 6
            Console.WriteLine("Step 6: Pruning tree");
            Prune();
        }

        private bool UndefinedCostsInRoot()
        {
            var worldsInRoot = Root.possibleWorlds.OfType<World>();
            return worldsInRoot.Any(w => w.objectiveCost.Type == CostType.Undefined);
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
                .GroupBy(edge => edge.action)
                .Select(group =>
                {
                    var actionOwner = group.First().action.owner;

                    // Costs from accessible worlds' children
                    var accessibleChildCosts = state.accessibility.GetAccessibleWorlds(actionOwner, w, includeCutEdges:false)
                        .OfType<World>()
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
    }
}
