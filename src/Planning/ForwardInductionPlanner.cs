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

        public Graph Plan()
        {
            // init
            this.Graph = new Graph(task);
            // State s0i = task.initialState.GetAssociatedLocal(planningAgent);
            Graph.root = Node.CreateRootNode(task.initialState);
            Graph.frontier.Enqueue(Graph.root);

            Console.WriteLine("Building search tree");
            this.BuildTree();

            Console.WriteLine("Computing costs");
            this.ComputeCosts();

            // if root worlds don't have fixed costs, iterate on depth
            HashSet<World> worldsInRoot = Graph.root.state.possibleWorlds.Cast<World>().ToHashSet();
            while (worldsInRoot.Any(w => w.cost.value.HasValue && w.cost.isRange))
            {
                Graph.leafNodes.Clear();

                Console.WriteLine("Iterating on cutoff depth. Expanding tree further.");

                this.BuildTree();

                Console.WriteLine("Reomputing costs");
                this.ComputeCosts();
            }
            
            Console.WriteLine("Pruning tree");

            this.Prune();
            return Graph;
        }


        // This one is without AND-OR, though we still do perspective shifts before product update
        public Graph BuildTree()
        {
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
                    //foreach (State si in s.state.PerspectiveShift(action.owner))
                    //{
                        if (s.state.IsApplicable(action, this.task))
                        //if (si.IsApplicable(action))
                        {
                            //State sPrime = s.state.ProductUpdate(action, si.designatedWorlds);
                            State sPrime = s.state.ProductUpdate(action, null, this.task);
                            sPrimeNode = new Node(sPrime, s, action);

                            if (cutoffDepth == int.MaxValue)
                            {
                                if (sPrime.HasGoalWorld(this.task.goalFormula))
                                {
                                    cutoffDepth = sPrimeNode.depth;
                                }
                            }

                            Graph.frontier.Enqueue(sPrimeNode);
                        }
                    //}
                }
            }

            foreach (Node node in Graph.frontier)
            {
                if (node.depth == cutoffDepth)
                {
                    Graph.leafNodes.Add(node);
                }
            }

            return Graph;
        }


        //todo: Instead of using two hashset to keep track of nodes to update, use a queue.
        public void ComputeCosts()
        {
            // Bottom-up traversal. Starting from leaves
            var nodesToUpdateNow = Graph.leafNodes;

            HashSet<Node> nodesToUpdateNext = new HashSet<Node>();

            while (nodesToUpdateNow.Any())
            {
                foreach (Node node in nodesToUpdateNow)
                {
                    foreach (World world in node.state.possibleWorlds)
                    {
                        this.ComputeWorldCost(node, world);
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
            HashSet<World> worldsInRoot = Graph.root.state.possibleWorlds.Cast<World>().ToHashSet();
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
                if (!w.HasAnyApplicableEvent(this.task, node.state))
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
                        // update only if range cost or no cost
                        if (!w.cost.value.HasValue || w.cost.isRange)
                        {
                            w.cost.value = fixedCostEdges.MinBy(x => x.cost.value).cost.value;
                            w.cost.isRange = false;
                        }
                    }
                    else
                    {
                        // update only if range cost or no cost
                        if (!w.cost.value.HasValue || w.cost.isRange)
                        {
                            w.cost.value = w.outgoingEdges.MinBy(x => x.cost.value).cost.value;
                            w.cost.isRange = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the cost on the incoming edge of a world, after the world cost has been computed.
        /// </summary>
        public void ComputeEdgeCost(World w)
        {
            // update only if cost is undefined or is range cost
            if (!w.incomingEdge.cost.value.HasValue || w.incomingEdge.cost.isRange)
            {
                if (w.cost.value == ushort.MaxValue)
                {
                    w.incomingEdge.cost.value = ushort.MaxValue;
                }
                else
                {
                    w.incomingEdge.cost.value = (ushort?)(w.cost.value + 1);
                }
                w.incomingEdge.cost.isRange = w.cost.isRange;
            }
        }

        public void Prune()
        {
            // Top-down traversal (BFS). Starting from root.
            Queue<Node> Q = new Queue<Node>();
            Q.Enqueue(Graph.root);
            Node node;
            Agent agent;

            while (Q.Any())
            {
                node = Q.Dequeue();

                if (node.depth > 0)
                {
                    foreach (World world in node.state.possibleWorlds)
                    {
                        if (world.incomingEdge.isPruned)
                        {
                            // Prune world and all its outgoing edges
                            world.isPruned = true;
                            foreach (WorldEdge edge in world.outgoingEdges)
                            {
                                edge.isPruned = true;
                            }
                        }
                    }
                }

                foreach (World world in node.state.possibleWorlds)
                {
                    if (world.isPruned) { continue; }

                    this.ComputeWorldAgentCosts(node, world);
                    foreach (WorldEdge edge in world.outgoingEdges)
                    {
                        // Prune outgoing edges with cost(w, i:a) > cost(w, i)
                        agent = edge.action.owner;
                        if (world.worldAgentCost[agent].value < edge.cost.value)
                        {
                            edge.isPruned = true;
                        }
                    }
                }

                //  Enqueue children
                foreach (Node child in node.children)
                {
                    Q.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// After costs for all worlds in state have been computed, calculate the world cost for each agent, cost(w,i), for all worlds in a state.
        /// </summary>
        public void ComputeWorldAgentCosts(Node node, World w)
        {
            HashSet<World> accessibleUnprunedWorlds;
            Cost worldAgentCost;

            if (!w.cost.value.HasValue)
            {
                throw new Exception("All worlds must have a defined cost(w) before computing world-agent costs");
            }
            foreach (Agent agent in node.state.accessibility.graph.Keys)
            {
                accessibleUnprunedWorlds = node.state.accessibility.GetAccessibleWorlds(agent, w).Cast<World>().Where(w => !w.isPruned).ToHashSet();
                var worldsWithRangeCosts = accessibleUnprunedWorlds.Where(x => x.cost.isRange);

                // cost(w,i) = max{v in v ~ w} cost(v)
                // Range costs are always greater than fixed costs, so we try to find the max there first
                if (worldsWithRangeCosts.Any())
                {
                    worldAgentCost.value = worldsWithRangeCosts.MaxBy(x => x.cost.value).cost.value;
                    worldAgentCost.isRange = true;
                }
                else
                {
                    worldAgentCost.value = accessibleUnprunedWorlds.MaxBy(x => x.cost.value).cost.value;
                    worldAgentCost.isRange = false;
                }
                w.worldAgentCost.Add(agent, worldAgentCost);
            }
        }
    }
}
