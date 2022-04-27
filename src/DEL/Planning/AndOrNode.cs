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

        public ushort Id => this.id;
        public State state;
        public NodeStatus status;
        public NodeType type;
        public Node parent;
        public Action actionFromParent;
        public HashSet<Node> children = new HashSet<Node>();
        public bool isRoot;
        public ushort cost = ushort.MaxValue;

        public Node(State state, Node parent, NodeType type, Action actionFromParent=null)
        {
            if (parent != null && parent.type == type)
            {
                throw new Exception("Parent of AND node must be and OR node and vice versa.");
            }
            this.state = state;
            this.parent = parent;
            this.type = type;
            this.actionFromParent = actionFromParent;
            this.status = NodeStatus.Undetermined;
            this.id = Counter;
            Counter++;
        }

        public bool Equals(Node other)
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