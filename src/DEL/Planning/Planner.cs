using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class Planner
    {
        private Graph Graph;

        /// <summary>
        /// The planning task to solve
        /// </summary>
        private PlanningTask task;

        public Planner(PlanningTask task)
        {
            this.task = task;
        }

        public void Plan()
        {
            Init();

            Node s;
            State sJ;
            Node sPrime;
            Node newGlobal;

            while (Graph.frontier.Count > 0)
            {
                s = Graph.frontier.Dequeue();

                Graph.LeafNodes.Add(s);

                foreach (Action action in task.actions)
                {
                    sJ = s.state.GetAssociatedLocal(action.owner);
                    sPrime = new Node(sJ.ProductUpdate(action), s, NodeType.And, action);                   

                    // Continue if action was not applicable or if s' already exists in AndNodes
                    if (sPrime == null || !Graph.AddAndNode(sPrime)) continue;

                    Graph.LeafNodes.Add(sPrime);

                    foreach (State global in sPrime.state.Globals())
                    {
                        newGlobal = new Node(global, sPrime, NodeType.Or);
                        if (!Graph.AddOrNode(newGlobal)) continue;

                        if (task.goalFormula.Evaluate(sPrime.state))
                        {
                            Graph.UpdateSolvedDead(newGlobal);
                        }
                        else
                        {
                            Graph.frontier.Enqueue(newGlobal);
                        }
                    }
                }
                Graph.UpdateSolvedDead(s);
                Graph.UpdateLeafNodes();

                if (Graph.root.status == NodeStatus.Solved)
                {
                    // extract policy
                }
                if (Graph.root.status == NodeStatus.Dead)
                {
                    throw new Exception("Root node is dead. Planning failed.");
                }
            }
            throw new Exception("Root node is dead. Planning failed.");
        }

        public void Init()
        {
            this.Graph = new Graph(task);

            Node newNode;

            foreach (State global in Graph.root.state.Globals())
            {
                newNode = new Node(global, Graph.root, NodeType.Or);

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

        public void ExtractPolicy()
        {

        }

        /// <summary>
        /// Performs a bottom-up iterative traversal of the graph to assign costs to nodes, as defined by DEL-implementation paper.
        /// </summary>
        public void AssignCosts()
        {

            foreach (Node node in this.Graph.LeafNodes)
            {
                node.cost = 0;
            }

            // This hashset contains the nodes that we assign the cost to in the i'th iteration
            // Initially, it contain the leaf nodes. Then the nodes are iteratively replaced by their parents.
            // If the parent of a node is null (root), the cost is updated and the node is removed from the set.
            HashSet<Node> nodesToUpdate = new HashSet<Node>(Graph.LeafNodes);

            ushort i = 1;

            while (nodesToUpdate.Any())
            {
                // todo: update nodes to their parents. 
                foreach (Node node in nodesToUpdate)
                {
                    if (node.cost == i - 1)
                    {
                        if (node.parent != null)
                        {
                            if (node.parent.type == NodeType.Or && node.parent.cost == ushort.MaxValue)
                            {
                                node.parent.cost = i;                            
                            }

                            if (node.parent.type == NodeType.And)
                            {
                                // If all children have defined costs, assign cost of child with max cost
                                if (node.parent.children.All(x => x.cost != ushort.MaxValue))
                                {
                                    node.parent.cost = node.parent.children.MaxBy(x => x.cost).cost;
                                }
                            }
                        }
                        else
                        {
                            //todo
                        }
                    }
                }

                i++;
            }
        }
    }
}
