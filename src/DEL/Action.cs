using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Action
    {
        public HashSet<IWorld> events;
        public HashSet<IWorld> designatedEvents;
        public AccessibilityRelation accessibility;

        public Action(HashSet<IWorld> events, HashSet<IWorld> designatedEvents, AccessibilityRelation accessibility)
        {
            this.events = events ?? throw new ArgumentNullException(nameof(events));

            if (designatedEvents == null || designatedEvents.IsSubsetOf(events))
            {
                this.designatedEvents = designatedEvents ?? new HashSet<IWorld>();
            }
            else
            {
                throw new ArgumentException("Set of designated events is not a subset of event set.");
            }

            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }

        public Action(HashSet<IWorld> events, HashSet<IWorld> designatedEvents, ICollection<Agent> agents)
        {
            this.events = events ?? throw new ArgumentNullException(nameof(events));

            if (designatedEvents == null || designatedEvents.IsSubsetOf(events))
            {
                this.designatedEvents = designatedEvents ?? new HashSet<IWorld>();
            }
            else
            {
                throw new ArgumentException("Set of designated events is not a subset of event set.");
            }

            if (agents == null) throw new ArgumentNullException(nameof(agents));

            AccessibilityRelation acs = new AccessibilityRelation(agents, events);
            this.accessibility = acs;
        }
    }
}
