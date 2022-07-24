using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class AndOrNode
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
        public AndOrNode parent;
        public Action actionFromParent;
        public HashSet<AndOrNode> children = new HashSet<AndOrNode>();
        public bool isRoot;
        public ushort? cost;

        public AndOrNode(State state, AndOrNode parent, NodeType type, Action actionFromParent=null)
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

        public static AndOrNode RootNode(State state)
        {
            AndOrNode root = new AndOrNode(state, null, NodeType.And);
            root.isRoot = true;
            return root;
        }

        public bool Equals(AndOrNode other)
        {
            return this.type == other.type && this.state.Equals(other.state);
        }
    }

    public enum NodeStatus
    {
        Solved = 0,
        Dead = 1,
        Undetermined = 2
    }

    public enum NodeType
    {
        And = 0,
        Or = 1,
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