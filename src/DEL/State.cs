using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using implicit_coordination.DEL;

namespace ImplicitCoodrination.DEL
{
    public class State
    {
        public HashSet<World> possibleWorlds;

        public HashSet<World> designatedWorlds;

        public IDictionary<Agent, HashSet<Tuple<World, World>>> accessibility;

        public IDictionary<World, BitVector32> assignment;

        public State(
            HashSet<World> possibleWorlds,
            HashSet<World> designatedWorlds,
            IDictionary<Agent, HashSet<Tuple<World, World>>> indistinguishability,
            IDictionary<World, BitVector32> assignment)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));
            this.designatedWorlds = designatedWorlds ?? throw new ArgumentNullException(nameof(designatedWorlds));
            this.accessibility = indistinguishability ?? throw new ArgumentNullException(nameof(indistinguishability));
            this.assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
        }
    }
}
