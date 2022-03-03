using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ImplicitCoodrination.DEL
{
    public class State
    {
        public HashSet<World> possibleWorlds;

        public HashSet<World> designatedWorlds;

        public IDictionary<Agent, HashSet<Tuple<World, World>>> accessibility;

        //public IDictionary<World, BitVector32> assignment;

        public State(
            HashSet<World> possibleWorlds,
            HashSet<World> designatedWorlds,
            IDictionary<Agent, HashSet<Tuple<World, World>>> accessibility,
            IDictionary<World, BitVector32> assignment)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));
            this.designatedWorlds = designatedWorlds ?? throw new ArgumentNullException(nameof(designatedWorlds));
            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
            //this.assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
        }

        public bool GetValuation(World w, Proposition p)
        {
            return w.valuation[p.id];
        }

        public void SetValuation(World w, Proposition p, bool value)
        {
            w.valuation[p.id] = value;
        }
    }
}