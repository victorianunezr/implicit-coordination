using System;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class ForwardInductionPlanner : IPlanner
    {
        private AndOrGraph Graph;

        /// <summary>
        /// The planning task to solve
        /// </summary>
        private PlanningTask task;

        public ForwardInductionPlanner(PlanningTask task)
        {
            this.task = task;
        }

        public void Plan()
        {
        }

        /// <summary>
        /// Builds the tree up to a cutoff depth.
        /// The cutoff depth is where we first find a goal state. We iterate on the depth until we have finite costs on the root's designated worlds.
        /// </summary>
        public void BuildTree()
        {
            Init();

            AndOrNode s;
            State sJ;
            AndOrNode sPrime;
            AndOrNode newGlobal;

            int cutoffDepth = int.MaxValue;

            while (Graph.frontier.Count > 0)
            {
                if (Graph.frontier.Peek().depth >= cutoffDepth)
                {
                    // stop expanding if we passed the cutoff depth
                    break;
                }

                s = Graph.frontier.Dequeue();

                Graph.SolvedLeafNodes.Add(s);

                foreach (Action action in task.actions)
                {
                    sJ = s.state.GetAssociatedLocal(action.owner);
                    sPrime = new AndOrNode(sJ.ProductUpdate(action), s, NodeType.And, action);

                    // Continue if action was not applicable or if s' already exists in AndNodes
                    if (sPrime == null || !Graph.AddAndNode(sPrime)) continue;

                    foreach (State global in sPrime.state.Globals())
                    {
                        newGlobal = new AndOrNode(global, sPrime, NodeType.Or);
                        if (!Graph.AddOrNode(newGlobal)) continue;

                        if (task.goalFormula.Evaluate(global))
                        {
                            // set cutoff depth where we find a goal state
                            cutoffDepth = newGlobal.depth;
                        }
                        else
                        {
                            Graph.frontier.Enqueue(newGlobal);
                        }
                    }
                }
                Graph.UpdateLeafNodes();

            }

        }

        // This one is without AND-OR, though we still do perspective shifts before product update
        public void BuildTree(Agent planningAgent)
        {
            // init
            Graph Graph = new Graph(task);
            State s0i = task.initialState.GetAssociatedLocal(planningAgent);
            Graph.root = Node.CreateRootNode(s0i);
            Graph.frontier.Enqueue(Graph.root);

            int cutoffDepth = int.MaxValue;

            Node s;
            State perspectiveShifted;

            while (Graph.frontier.Count > 0)
            {
                s = Graph.frontier.Dequeue();

                foreach (Action action in task.actions)
                {
                    
                }
            }
        }

        public void Init()
        {
            this.Graph = new AndOrGraph(task);

            AndOrNode newNode;

            foreach (State global in Graph.root.state.Globals())
            {
                newNode = new AndOrNode(global, Graph.root, NodeType.Or);

                if (!Graph.AddOrNode(newNode)) continue;

                if (task.goalFormula.Evaluate(Graph.root.state))
                {
                    Graph.UpdateSolvedDead(newNode);
                }
                else
                {
                    Graph.frontier.Enqueue(newNode);
                }
            }
        }

        public void ComputeCosts()
        {

        }

        /// <summary>
        /// Computes the world costs. Only to be applied on AND nodes
        /// </summary>
        /// <param name="s"></param>
        /// <param name="w"></param>
        public void ComputeWorldCost(State s, World w)
        {
            if (this.task.goalFormula.Evaluate(s, w))
            {
                w.cost = 0;
            }
            else
            {
                // If no applicable actions, assign infinity
                if (task.actions.Any(x => !s.IsApplicable(x)))
                {
                    w.
                }
            }
        }
    }
}
