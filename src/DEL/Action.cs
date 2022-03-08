using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Action
    {
        public HashSet<Event> events;
        public HashSet<Event> designatedEvents;
        public AccessibilityRelation accessibility;

        public Action(HashSet<Event> events, HashSet<Event> designatedEvents, AccessibilityRelation accessibility)
        {
            this.events = events ?? throw new ArgumentNullException(nameof(events));

            if (!(designatedEvents == null || designatedEvents.IsSubsetOf(events)))
            {
                this.designatedEvents = designatedEvents ?? new HashSet<Event>();
            }
            else
            {
                throw new ArgumentException("Set of designated events is not a subset of event set.");
            }

            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }
    }
}
