using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class ForwardInductionPlanner : IPlanner
    {
        private Graph Graph;

        /// <summary>
        /// The planning task to solve
        /// </summary>
        private PlanningTask task;

        public ForwardInductionPlanner(PlanningTask task)
        {
            this.task = task;
        }

        public Graph Plan(Agent planningAgent)
        {
            Graph graph = this.BuildTree(planningAgent);
            this.ComputeCosts(graph);
            return graph;
        }

        ///// <summary>
        ///// Builds the tree up to a cutoff depth.
        ///// The cutoff depth is where we first find a goal state. We iterate on the depth until we have finite costs on the root's designated worlds.
        ///// </summary>
        //public void BuildTree()
        //{
        //    Init();

        //    AndOrNode s;
        //    State sJ;
        //    AndOrNode sPrime;
        //    AndOrNode newGlobal;

        //    int cutoffDepth = int.MaxValue;

        //    while (Graph.frontier.Count > 0)
        //    {
        //        if (Graph.frontier.Peek().depth >= cutoffDepth)
        //        {
        //            // stop expanding if we passed the cutoff depth
        //            break;
        //        }

        //        s = Graph.frontier.Dequeue();

        //        Graph.SolvedLeafNodes.Add(s);

        //        foreach (Action action in task.actions)
        //        {
        //            sJ = s.state.GetAssociatedLocal(action.owner);
        //            sPrime = new AndOrNode(sJ.ProductUpdate(action), s, NodeType.And, action);

        //            // Continue if action was not applicable or if s' already exists in AndNodes
        //            if (sPrime == null || !Graph.AddAndNode(sPrime)) continue;

        //            foreach (State global in sPrime.state.Globals())
        //            {
        //                newGlobal = new AndOrNode(global, sPrime, NodeType.Or);
        //                if (!Graph.AddOrNode(newGlobal)) continue;

        //                if (task.goalFormula.Evaluate(global))
        //                {
        //                    // set cutoff depth where we find a goal state
        //                    cutoffDepth = newGlobal.depth;
        //                }
        //                else
        //                {
        //                    Graph.frontier.Enqueue(newGlobal);
        //                }
        //            }
        //        }
        //        Graph.UpdateLeafNodes();

        //    }
        //}

        // This one is without AND-OR, though we still do perspective shifts before product update
        public Graph BuildTree(Agent planningAgent)
        {
            // init
            this.Graph = new Graph(task);
            State s0i = task.initialState.GetAssociatedLocal(planningAgent);
            Graph.root = Node.CreateRootNode(s0i);
            Graph.frontier.Enqueue(Graph.root);

            int cutoffDepth = int.MaxValue;

            Node s;
            Node sPrimeNode;

            while (Graph.frontier.Any())
            {
                if (Graph.frontier.Peek().depth >= cutoffDepth)
                {
                    // stop expanding if we passed the cutoff depth
                    // because we are doing BFS, nodes are expanded level by level, so nodes are expanded in order by depth
                    break;
                }

                s = Graph.frontier.Dequeue();

                foreach (Action action in task.actions)
                {
                    foreach (State si in s.state.PerspectiveShift(action.owner))
                    {
                        if (si.IsApplicable(action))
                        {
                            State sPrime = s.state.ProductUpdate(action, si.designatedWorlds);
                            sPrimeNode = new Node(sPrime, s, action);

                            if (cutoffDepth == int.MaxValue)
                            {
                                if (sPrime.IsGoalState(this.task.goalFormula))
                                {
                                    cutoffDepth = sPrimeNode.depth;
                                }
                            }

                            if (sPrimeNode.depth == cutoffDepth)
                            {
                                Graph.leafNodes.Add(sPrimeNode);
                            }

                            Graph.frontier.Enqueue(sPrimeNode);
                        }
                    }
                }
            }

            return Graph;
        }

        //public void Init()
        //{
        //    this.Graph = new AndOrGraph(task);

        //    AndOrNode newNode;

        //    foreach (State global in Graph.root.state.Globals())
        //    {
        //        newNode = new AndOrNode(global, Graph.root, NodeType.Or);

        //        if (!Graph.AddOrNode(newNode)) continue;

        //        if (task.goalFormula.Evaluate(Graph.root.state))
        //        {
        //            Graph.UpdateSolvedDead(newNode);
        //        }
        //        else
        //        {
        //            Graph.frontier.Enqueue(newNode);
        //        }
        //    }
        //}

        public void ComputeCosts(Graph graph)
        {
            var nodesToUpdateNow = graph.leafNodes;

            HashSet<Node> nodesToUpdateNext = new HashSet<Node>();

            while (nodesToUpdateNow.Any())
            {
                foreach (Node node in nodesToUpdateNow)
                {
                    foreach (World world in node.state.possibleWorlds)
                    {
                        this.ComputeWorldCost(node, world);
                        this.ComputeWorldAgentCosts(node, world);
                        if (!node.isRoot)
                        {
                           this.ComputeEdgeCost(world);
                        }
                    }
                    if (node.parent != null)
                    {
                        nodesToUpdateNext.Add(node.parent);
                    }
                }

                nodesToUpdateNow = new HashSet<Node>(nodesToUpdateNext);
                nodesToUpdateNext.Clear();
            }

            // check that all worlds have an assigned cost at root
            HashSet<World> worldsInRoot = graph.root.state.possibleWorlds.Cast<World>().ToHashSet();
            if (worldsInRoot.Any(w => !w.cost.value.HasValue))
            {
                throw new Exception("Some worlds in root have undefined value");
            }

        }

        /// <summary>
        /// Computes the world costs.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="w"></param>
        public void ComputeWorldCost(Node node, World w)
        {
            if (this.task.goalFormula.Evaluate(node.state, w))
            {
                w.cost.value = 0;
            }
            else
            {
                // If no applicable events on the world, assign infinity (max value)
                if (!task.actions.Any(x => w.HasAnyApplicableEvent(x, node.state)))
                {
                    w.cost.value = ushort.MaxValue;
                    w.cost.isRange = true;
                }
                // If it's a leaf node and there are applicable events, cost is 1+
                else if (Graph.leafNodes.Contains(node))
                {
                    w.cost.value = 1;
                    w.cost.isRange = true;
                }
                // If not a leaf node, assing min cost of all outgoing edges
                else
                {
                    if (w.outgoingEdges.Any(x => !x.cost.value.HasValue))
                    {
                        throw new Exception("All outgoing edges must have a defined cost.");
                    }

                    var fixedCostEdges = w.outgoingEdges.Where(x => !x.cost.isRange);

                    // The minimum cost must be a fixed cost edge. If there are none, range costs are considered.
                    if (fixedCostEdges.Any())
                    {
                        w.cost.value = fixedCostEdges.MinBy(x => x.cost.value).cost.value;
                        w.cost.isRange = false;
                    }
                    else
                    {
                        w.cost.value = w.outgoingEdges.MinBy(x => x.cost.value).cost.value;
                        w.cost.isRange = true;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the cost on the incoming edge of a world, after the world cost has been computed.
        /// </summary>
        /// <param name=""></param>
        public void ComputeEdgeCost(World w)
        {
            w.incomingEdge.cost.value = (ushort?)(w.cost.value + 1);
            w.incomingEdge.cost.isRange = w.cost.isRange;
        }

        /// <summary>
        /// After costs for all worlds in state have been computed, calculate the world cost for each agent, cost(w,i), for all worlds in a state.
        /// </summary>
        public void ComputeWorldAgentCosts(Node node, World w)
        {
            HashSet<World> accessibleWorlds;
            Cost worldAgentCost;

            if (!w.cost.value.HasValue)
            {
                throw new Exception("All worlds must have a defined cost(w) before computing world-agent costs");
            }
            foreach (Agent agent in node.state.accessibility.graph.Keys)
            {
                accessibleWorlds = node.state.accessibility.GetAccessibleWorlds(agent, w).Cast<World>().ToHashSet();
                var worldsWithRangeCosts = accessibleWorlds.Where(x => x.cost.isRange);

                // cost(w,i) = max{v in v ~ w} cost(v)
                // Range costs are always greater than fixed costs, so we try to find the max there first
                if (worldsWithRangeCosts.Any())
                {
                    worldAgentCost.value = worldsWithRangeCosts.MaxBy(x => x.cost.value).cost.value;
                    worldAgentCost.isRange = true;
                }
                else
                {
                    worldAgentCost.value = accessibleWorlds.MaxBy(x => x.cost.value).cost.value;
                    worldAgentCost.isRange = false;
                }
                w.worldAgentCost.Add(agent, worldAgentCost);
            }
        }
    }
}
