using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ImplicitCoordination.DEL
{
    public class EpistemicModel
    {
        public HashSet<IWorld> possibleWorlds;

        public HashSet<IWorld> designatedWorlds;

        public AccessibilityRelation accessibility;

        public EpistemicModel()
        {
            this.possibleWorlds = new HashSet<IWorld>();
            this.designatedWorlds = new HashSet<IWorld>();
            this.accessibility = new AccessibilityRelation();
        }

        public EpistemicModel(
            HashSet<IWorld> possibleWorlds,
            HashSet<IWorld> designatedWorlds,
            AccessibilityRelation accessibility)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));

            if (designatedWorlds == null || designatedWorlds.IsSubsetOf(possibleWorlds))
            {
                this.designatedWorlds = designatedWorlds ?? new HashSet<IWorld>();
            }
            else
            {
                throw new ArgumentException("Set of designated worlds is not a subset of possible worlds.");
            }

            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }


        public EpistemicModel(HashSet<IWorld> possibleWorlds,
                     HashSet<IWorld> designatedWorlds,
                     ICollection<Agent> agents)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));

            if (designatedWorlds == null || designatedWorlds.IsSubsetOf(possibleWorlds))
            {
                this.designatedWorlds = designatedWorlds ?? new HashSet<IWorld>();
            }
            else
            {
                throw new ArgumentException("Set of designated worlds is not a subset of possible worlds.");
            }

            if (agents == null) throw new ArgumentNullException(nameof(agents));

            AccessibilityRelation acs = new AccessibilityRelation(agents, possibleWorlds);
            this.accessibility = acs;
        }
    }
}
