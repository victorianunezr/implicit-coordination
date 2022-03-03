using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ImplicitCoodrination.DEL
{
    public interface IEpistemicModel
    {
        public HashSet<World> PossibleWorlds { get; set; }

        public IDictionary<Agent, HashSet<Tuple<World,World>>> Indistinguishability { get; set; }

        public IDictionary<World, BitVector32> Assignment { get; set; }
    }
}
