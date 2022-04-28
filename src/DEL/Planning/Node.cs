using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class Node
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        /// <summary>
        /// The depth of a node. An AND node and its associated globals have the same depth.
        /// The root (AND node) and its associated globals (OR nodes) have depth 0.
        /// </summary>
        public int depth;
        public ushort Id => this.id;
        public State state;
        public NodeStatus status;
        public NodeType type;
        public Node parent;
        public Action actionFromParent;
        public HashSet<Node> children = new HashSet<Node>();
        public bool isRoot;
        public ushort? cost;

        public Node(State state, Node parent, NodeType type, Action actionFromParent=null)
        {
            if (parent != null && parent.type == type)
            {
                throw new Exception("Parent of AND node must be and OR node and vice versa.");
            }
            this.state = state;
            if (parent != null)
            {
                this.parent = parent;
                // If new node is OR, the depth is the same as its parent
                if (parent.type == NodeType.And)
                {
                    this.depth = parent.depth;
                }
                else
                {
                    this.depth = parent.depth + 1;
                }
            }
            else
            {
                this.depth = 0;
            }
            this.type = type;
            this.actionFromParent = actionFromParent;
            this.status = NodeStatus.Undetermined;
            this.id = Counter;
            Counter++;
        }

        public static Node RootNode(State state)
        {
            Node root = new Node(state, null, NodeType.And);
            root.isRoot = true;
            return root;
        }

        public bool Equals(Node other)
        {
            return this.type == other.type && this.state.Equals(other.state);
        }
    }

    //todo: find out if there is a way to hash two bisimilar states into the same value
    //public class NodeComparator : IEqualityComparer<Node>
    //{
    //    public bool Equals(Node x, Node y)
    //    {
    //        if (x.type != y.type) return false;
    //        return x.state.Equals(y.state);
    //    }

    //    public int GetHashCode([DisallowNull] Node obj)
    //    {
    //        throw new NotImplementedException("Tried calling GetHashCode for Node");
    //    }
    //}
}