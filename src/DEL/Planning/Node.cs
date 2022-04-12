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

        public ushort Id => this.id;
        public State state;
        public NodeStatus status;
        public NodeType type;
        public Node parent;
        public Action actionFromParent;
        public HashSet<Node> children;
        public bool isRoot;

        public Node(State state, Node parent, NodeType type, Action actionFromParent)
        {
            this.state = state;
            this.parent = parent;
            this.type = type;
            this.actionFromParent = actionFromParent;
            this.id = Counter;
            Counter++;
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
}
