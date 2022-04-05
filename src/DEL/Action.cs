using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Action : EpistemicModel
    {
        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      AccessibilityRelation accessibility)
            : base(events, designatedEvents, accessibility)
        {
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      ICollection<Agent> agents)
            : base(events, designatedEvents, agents)
        {
        }
    }
}
