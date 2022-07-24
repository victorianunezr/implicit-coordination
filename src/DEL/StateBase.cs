using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class StateBase
    {
        public HashSet<IWorld> possibleWorlds;

        public HashSet<IWorld> designatedWorlds;

        public AccessibilityRelation accessibility;

        public StateBase(
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

    }
}
