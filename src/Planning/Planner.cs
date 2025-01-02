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
            Console.WriteLine("Building search tree");
            BuildTree();

            Console.WriteLine("Computing costs");
            ComputeCosts();

            // Step 3
            var worldsInRoot = Root.possibleWorlds.OfType<World>();
            if (worldsInRoot.All(w => w.objectiveCost.Type == CostType.Infinity))
            {
                // Problem not solvable. Give up
                return;
            }
            // if any root world has cost +, iterate on depth
            while (worldsInRoot.Any(w => w.objectiveCost.Type == CostType.Undefined))
            {
                Leaves.Clear();

                Console.WriteLine("Iterating on cutoff depth. Expanding tree further.");

                BuildTree();

                Console.WriteLine("Recomputing costs");
                ComputeCosts();
            }
            
            Console.WriteLine("Pruning tree");

            Prune();
        }


        public void BuildTree()
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

        private void ComputeObjectiveWorldCost(State state, World w)
        {
            if (Problem.goalFormula.Evaluate(state, w))
            {
                w.objectiveCost = Cost.Finite(0);
            }
            else
            {
                // If no applicable events on the world, assign infinity (max value)
                if (!w.HasAnyApplicableEvent(Domain.actions, state))
                {
                    w.objectiveCost = Cost.Infinity();
                }
                // If it's a leaf node and there are applicable events, cost is +
                else if (Leaves.Contains(state))
                {
                    w.objectiveCost = Cost.Undefined();
                }
                // If not a leaf node, assing min cost of all outgoing edges
                else
                {
                    // Group edges by action
                    var actionGroups = w.outgoingEdges
                        .GroupBy(edge => edge.action)
                        .Select(group => group.ToList())
                        .ToList();

                    // Compute max cost for each action group
                    var actionCosts = actionGroups
                        .Select(group => group.Max(edge => edge.childWorld.objectiveCost))
                        .ToList();

                    // Find the minimum among these maximum costs
                    var minActionCost = actionCosts.Min().Value;

                    // Apply the cost formula: 1 + min(max(child_costs))
                    w.objectiveCost = Cost.Finite(minActionCost + 1);
            }
        }
    }
}
