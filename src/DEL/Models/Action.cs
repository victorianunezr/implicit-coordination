using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    //todo: fully implement action owner
    public class Action : EpistemicModel
    {
        public HashSet<Agent> owners { get; set; } = new();
        public string name;

        public Action()
        {
            this.owners = new HashSet<Agent>();
            this.possibleWorlds = new HashSet<IWorld>();
            this.designatedWorlds = new HashSet<IWorld>();
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      AccessibilityRelation accessibility,
                      string name,
                      HashSet<Agent> owners)
            : base(events, designatedEvents, accessibility)
        {
            this.name = name;
            this.owners = owners;
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      AccessibilityRelation accessibility,
                      string name,
                      Agent owner)
            : base(events, designatedEvents, accessibility)
        {
            this.name = name;
            this.owners.Add(owner);
        }

        public Action(HashSet<IWorld> events,
                      HashSet<IWorld> designatedEvents,
                      ICollection<Agent> agents,
                      string name,
                      Agent owner)
            : base(events, designatedEvents, agents)
        {
            this.name = name;
            this.owners.Add(owner);
        }       
    }
}
