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

        /// <summary>
        /// Returns a world with automatically incremented id and BitArray with proposition valuations.
        /// </summary>
        /// <param name="valuation"></param>
        /// <remarks>
        /// Least significant bit in valuation corresponds to proposition with id=0.
        /// Instantiating a World without passing a valuation results in a world with all values set to false.
        /// </remarks>
        public World(uint valuationData=0)
        {
            this.valuation = new BitArray(valuationData);
            this.id = Counter;
            Counter++;
        }
    }
}