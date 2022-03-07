using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ImplicitCoordination.DEL
{
    public class State
    {
        public HashSet<World> possibleWorlds;

        public HashSet<World> designatedWorlds;

        public AccessibilityRelation accessibility;

        //public IDictionary<World, BitVector32> assignment;

        public State(
            HashSet<World> possibleWorlds,
            HashSet<World> designatedWorlds,
            AccessibilityRelation accessibility)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));
            this.designatedWorlds = designatedWorlds;
            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }

        public bool GetValuation(World w, Proposition p)
        {
            return w.valuation.GetValue(p.id);
        }

        public void SetValuation(World w, Proposition p, bool value)
        {
            w.valuation.SetValue(p.id, value);
        }
    }
}