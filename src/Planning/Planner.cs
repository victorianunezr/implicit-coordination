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

            // if root worlds don't have fixed costs, iterate on depth
            HashSet<World> worldsInRoot = Root.possibleWorlds.Cast<World>().ToHashSet();
            while (worldsInRoot.Any(w => w.cost.value.HasValue && w.cost.isRange))
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
        private void ComputeCosts()
        {
            throw new NotImplementedException();
        }
    }
}
