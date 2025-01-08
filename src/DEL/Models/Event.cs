using ImplicitCoordination.DEL.utils;
using ImplicitCoordination.Planning;
using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Event : IWorld
    {
        public string Name { get; set; }
        private static ushort Counter = 0;
        private readonly ushort id;

        /// <summary>
        /// Effect is a dictionary mapping a predicate to their valuation.
        /// </summary>
        /// <remarks>
        /// If some p is not in the dict, and if w |= p before applying the event,
        /// the resulting world w' after applying the event will have w' |= p.
        /// If a postcondition is a negated predicate ~p, set post[p] to false.
        /// If dictionary is null, valuation remains the same as in the source state.
        /// </remarks>
        public IDictionary<Predicate, bool> effect;

        public ushort Id => this.id;
        // public ulong TruePropositions => throw new NotImplementedException();

        /// <summary>
        /// Precondition Formula that is evaluated in each world when applying the product update.
        /// </summary>
        public Formula pre { get; set; }

        public Event()
        {
            this.effect = new Dictionary<Predicate, bool>();
            Counter++;
        }

        public Event(Formula pre, IDictionary<Predicate, bool> effect=null)
        {
            this.pre = pre;
            this.effect = effect;
            this.id = Counter;
            Counter++;
        }
    }
}