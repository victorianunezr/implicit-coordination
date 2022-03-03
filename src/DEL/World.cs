using System;
using System.Collections.Specialized;

namespace ImplicitCoodrination.DEL
{
    public class World
    {
        // valuation[i] gives truth value for atomic proposition with id i. BitVector32 only allows 32 propositions per world.
        public BitVector32 valuation;

        public World()
        {
        }
    }
}
