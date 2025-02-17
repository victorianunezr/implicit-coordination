using System;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class WorldEdge
    {
        public World childWorld;
        public World parentWorld;
        public Event parentEvent;
        public Action action;
        public Agent actingAgent;
        public Cost cost;
        public bool isPruned;

        public WorldEdge(World childWorld, World parentWorld, Event parentEvent, Action action, Agent actionOwner)
        {
            this.childWorld = childWorld;
            this.parentWorld = parentWorld;
            this.parentEvent = parentEvent;
            this.action = action;
            this.actingAgent = actionOwner;
        }
    }
}
