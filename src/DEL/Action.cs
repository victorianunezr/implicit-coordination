using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    //todo: fully implement action owner
    public class Action : EpistemicModel
    {
        public Agent owner;

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      AccessibilityRelation accessibility,
                      Agent owner=null)
            : base(events, designatedEvents, accessibility)
        {
            this.owner = owner;
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      ICollection<Agent> agents,
                      Agent owner=null)
            : base(events, designatedEvents, agents)
        {
            this.owner = owner;
        }
    }
}
