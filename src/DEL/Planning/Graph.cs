using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    /// <summary>
    /// Class handling graph structure. Generates new nodes while keeping track of parent/children relations and actions used.
    /// </summary>
    public class Graph
    {
        //private static NodeComparator NodeComparator = new NodeComparator();
        public Node root;

        /// <summary>
        /// Frontier containing OR nodes
        /// </summary>
        public Queue<Node> frontier;

        /// <summary>
        /// AND nodes are the states resulting from a perspective shift followed by a product update
        /// </summary>
        public HashSet<Node> AndNodes = new HashSet<Node>();

        /// <summary>
        /// OR nodes are the global states resulting from an AND node
        /// </summary>
        public HashSet<Node> OrNodes = new HashSet<Node>();

        /// <summary>
        /// The goal formula
        /// </summary>
        private Formula goalFormula;

        public Graph(PlanningTask task)
        {
            this.root = new Node(task.initialState, null, NodeType.And, null);
            this.root.isRoot = true;
            this.goalFormula = task.goalFormula;
            AddAndNode(this.root);
        }

        /// <summary>
        /// Creates a new AND node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from product update.</param>
        /// <param name="actionToParent">Action that lead to the new node being creted. Used to extract policy.</param>
        /// <param name="parent">Parent node is an OR node. The new node created will be added to the children of the parent.</param>
        private Node CreateAndNode(State state, Action actionToParent, Node parent)
        {
            Node node = new Node(state, parent, NodeType.And, actionToParent);
            AndNodes.Add(node);
            parent.children.Add(node);
            //AddNode(node);
            return node;
        }

        /// <summary>
        /// Adds a node to the set of AND nodes.
        /// </summary>
        /// <returns>True if the node was added, false otherwise.</returns>>
        public bool AddAndNode(Node newNode)
        {
            if (newNode.type != NodeType.And)
            {
                throw new IncorrectNodeTypeException();
            }

            // Add if equal node does not exist
            bool add = !this.AndNodes.Any(x => x.Equals(newNode));
            if (add)
            {
                if (!newNode.isRoot)
                {
                    if (newNode.parent == null)
                    {
                        throw new Exception("Parent of non-root AND node cannot be null.");
                    }
                    if (newNode.parent.type != NodeType.Or)
                    {
                        throw new Exception("Parent of non-root AND node must be an OR node.");
                    }
                    newNode.parent.children.Add(newNode);
                }
                AndNodes.Add(newNode);
                //AddNode(newNode);
            }
            return add;
        }

        /// <summary>
        /// Creates a new OR node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from list of globals of parent.</param>
        /// <param name="parent">Parent node is an AND node. The new node created will be added to the children of the parent.</param>
        private Node CreateOrNode(State state, Node parent)
        {
            if (state.designatedWorlds.Count != 1)
            {
                throw new Exception("State is not a global state. Cannot create OR node.");
            }

            Node node = new Node(state, parent, NodeType.Or, null);
            OrNodes.Add(node);
            parent.children.Add(node);
            //AddNode(node);
            return node;
        }

        /// <summary>
        /// Adds a node to the set of OR nodes.
        /// </summary>
        /// <returns>True if the node was added, false otherwise.</returns>>
        public bool AddOrNode(Node newNode)
        {
            if (newNode.type != NodeType.Or)
            {
                throw new IncorrectNodeTypeException();
            }
            if (newNode.state.designatedWorlds.Count != 1)
            {
                throw new Exception("State is not a global state. Cannot add to OR nodes.");
            }
            if (newNode.parent == null)
            {
                throw new Exception("Parent of OR node cannot be null.");
            }
            if (newNode.parent.type != NodeType.And)
            {
                throw new Exception("Parent of OR node must be an AND node.");
            }

            // Add if equal node does not exist
            bool add = !this.OrNodes.Any(x => x.Equals(newNode));
            if (add)
            {
                OrNodes.Add(newNode);
                //AddNode(newNode);
                newNode.parent.children.Add(newNode);
            }
            return add;
        }


        //public void AddNode(Node node)
        //{
        //    if (!nodes.ContainsKey(node.Id))
        //    {
        //        nodes.Add(node.Id, node);
        //    }
        //}

        //public Node GetNode(ushort nodeId)
        //{
        //    Node node;
        //    if (nodes.TryGetValue(nodeId, out node))
        //    {
        //        return node;
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException("Node not in graph");
        //    }
        //}

        public void UpdateSolvedDead(Node node)
        {
            if (node.status == NodeStatus.Undetermined)
            {
                if (IsSolved(node))
                {
                    node.status = NodeStatus.Solved;
                }
                else if (IsDead(node))
                {
                    node.status = NodeStatus.Dead;
                }
            }
        }

        public bool IsSolved(Node node)
        {
            if (node.type == NodeType.And)
            {
                foreach (Node child in node.children)
                {
                    if (child.status != NodeStatus.Solved) return false;
                }
                return true;
            }
            else
            {
                if (goalFormula.Evaluate(node.state)) return true;

                foreach (Node child in node.children)
                {
                    if (child.status == NodeStatus.Solved) return true;
                }
                return false;
            }
        }

        public bool IsDead(Node node)
        {
            if (node.type == NodeType.And)
            {
                foreach (Node child in node.children)
                {
                    if (child.status == NodeStatus.Dead) return true;
                }
                return false;
            }
            else
            {
                foreach (Node child in node.children)
                {
                    if (child.status != NodeStatus.Dead) return false;
                }
                return !goalFormula.Evaluate(node.state);
            }
        }

    }

    public class IncorrectNodeTypeException : Exception { }
}
