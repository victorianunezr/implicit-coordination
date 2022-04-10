using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    /// <summary>
    /// Class handling graph structure. Generates new nodes while keeping track of parent/children relations and actions used.
    /// </summary>
    public class Graph
    {
        public Node root;
        public Dictionary<ushort, Node> nodes;

        /// <summary>
        /// Frontier containing node ids
        /// </summary>
        public Queue<ushort> frontier;

        /// <summary>
        /// AND nodes are the states resulting from a perspective shift followed by a product update
        /// </summary>
        private HashSet<Node> AndNodes;

        /// <summary>
        /// OR nodes are the global states resulting from an AND node
        /// </summary>
        private HashSet<Node> OrNodes;


        public Graph(State initialState)
        {
            this.root = new Node(initialState, null, NodeType.And, null);
            this.root.isRoot = true;
        }

        /// <summary>
        /// Creates a new AND node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from product update.</param>
        /// <param name="actionToParent">Action that lead to the new node being creted. Used to extract policy.</param>
        /// <param name="parent">Parent node is an OR node. The new node created will be added to the children of the parent.</param>
        public Node CreateAndNode(State state, Action actionToParent, Node parent)
        {
            Node node = new Node(state, parent, NodeType.And, actionToParent);
            AndNodes.Add(node);
            parent.children.Add(node);
            AddNode(node);
            return node;
        }

        /// <summary>
        /// Creates a new OR node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from list of globals of parent.</param>
        /// <param name="parent">Parent node is an AND node. The new node created will be added to the children of the parent.</param>
        public Node CreateOrNode(State state, Node parent)
        {
            Node node = new Node(state, parent, NodeType.Or, null);
            OrNodes.Add(node);
            parent.children.Add(node);
            AddNode(node);
            return node;
        }

        public void AddNode(Node node)
        {
            if (!nodes.ContainsKey(node.Id))
            {
                nodes.Add(node.Id, node);
            }
        }

        public Node GetNode(ushort nodeId)
        {
            Node node;
            if (nodes.TryGetValue(nodeId, out node))
            {
                return node;
            }
            else
            {
                throw new KeyNotFoundException("Node not in graph");
            }
        }

        public Node DequeueFrontier()
        {
            ushort nodeId = this.frontier.Dequeue();
            return GetNode(nodeId);
        }
    }
}
