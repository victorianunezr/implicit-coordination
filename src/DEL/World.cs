using System;
using ImplicitCoordination.DEL.utils;

namespace ImplicitCoordination.DEL
{
    public class World
    {
        private static ushort Counter = 0;
        public ushort id;
        // valuation[i] gives truth value for atomic proposition with id i. BitVector32 only allows 32 propositions per world.
        public BitArray valuation;

        // Instantiating a World without passing a valuation results in a world with all values set to false.
        public World(BitArray valuation = new BitArray())
        {
            this.valuation = valuation;
            this.id = Counter;
            Counter++;
        }
    }
}