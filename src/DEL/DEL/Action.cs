using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    //todo: fully implement action owner
    public class Action : EpistemicModel
    {
        public Agent owner;
        public string name;

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      AccessibilityRelation accessibility,
                      string name,
                      Agent owner)
            : base(events, designatedEvents, accessibility)
        {
            this.name = name;
            this.owner = owner;
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      ICollection<Agent> agents,
                      string name,
                      Agent owner)
            : base(events, designatedEvents, agents)
        {
            this.name = name;
            this.owner = owner;
        }
    }
}
