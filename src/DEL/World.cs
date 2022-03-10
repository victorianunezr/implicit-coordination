﻿using ImplicitCoordination.DEL.utils;

namespace ImplicitCoordination.DEL
{
    public class World : IWorld
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        public ushort Id => this.id;

        // valuation[i] gives truth value for atomic proposition with id i. BitVector32 only allows 32 propositions per world.
        public BitArray valuation;

        // Parent world and event used to track source of new world generated during product update
        public World parentWorld;
        public Event parentEvent;


        /// <summary>
        /// Returns a world with automatically incremented id and BitArray with proposition valuations.
        /// </summary>
        /// <param name="valuation"></param>
        /// <remarks>
        /// Least significant bit in valuation corresponds to proposition with id=0.
        /// Instantiating a World without passing a valuation results in a world with all values set to false.
        /// </remarks>
        public World(ulong valuationData = 0, World parentWorld=null, Event parentEvent=null)
        {
            this.valuation = new BitArray(valuationData);
            this.parentWorld = parentWorld;
            this.parentEvent = parentEvent;
            this.id = Counter;
            Counter++;
        }

        public bool GetValuation(Proposition p)
        {
            return this.valuation.GetValue(p.id);
        }

        public void SetValuation(Proposition p, bool value)
        {
            this.SetValuation(p.id, value);
        }

        public void SetValuation(ushort propId, bool value)
        {
            this.valuation.SetValue(propId, value);
        }


        public bool IsValid(State s, Formula f)
        {
            return f.Evaluate(s, this);
        }

        public World Copy()
        {
            return new World(this.valuation.data);
        }

        /// <summary>
        /// Creates a child world with the provided world and event as parents.
        /// The child world is initialized with the parent world's valuation data.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public World CreateChild(Event e)
        {
            return new World(this.valuation.data, this, e);
        }
    }
}