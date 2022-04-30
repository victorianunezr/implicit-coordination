using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class Node
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        /// <summary>
        /// The depth of a node. Root has depth 0
        /// </summary>
        public int depth;
        public ushort Id => this.id;
        public State state;
        public NodeStatus status;
        public Node parent;
        public Action actionFromParent;
        public HashSet<Node> children = new HashSet<Node>();
        public bool isRoot;
        public ushort? cost;

        public Node(State state, Node parent, Action actionFromParent)
        {
            this.state = state;
            if (parent != null)
            {
                this.parent = parent;
                this.depth = parent.depth + 1;
            }
            else
            {
                throw new ArgumentNullException(nameof(parent));
            }
            this.actionFromParent = actionFromParent ?? throw new ArgumentNullException(nameof(actionFromParent));
            this.status = NodeStatus.Undetermined;
            this.id = Counter;
            Counter++;
        }

        // To be used only by root node
        private Node(State state)
        {
            this.state = state;
            this.depth = 0;
            this.status = NodeStatus.Undetermined;
            this.id = Counter;
            this.isRoot = true;
            Counter++;
        }

        public static Node CreateRootNode(State state)
        {
            return new Node(state);
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
}