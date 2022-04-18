using ImplicitCoordination.DEL.utils;
using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Event : IWorld
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        public ushort Id => this.id;
        public ulong TruePropositions => throw new NotImplementedException();

        /// <summary>
        /// Precondition Formula that is evaluated in each world when applying the product update.
        /// </summary>
        public Formula pre { get; set; }

        /// <summary>
        /// Postcondition is a dictionary mapping a proposition id to their valuation.
        /// </summary>
        /// <remarks>
        /// If some p.id is not in the dict, and if w |= p before applying the event,
        /// the resulting world w' after applying the event will have w' |= p.
        /// If a postcondition is a negated proposition ~p, set post[p.id] to false.
        /// If dictionary is null, valuation remains the same as in the source state.
        /// </remarks>
        public IDictionary<ushort, bool>? post;

        public Event(Formula pre, IDictionary<ushort, bool> post=null)
        {
            this.pre = pre;
            this.post = post;
            this.id = Counter;
            Counter++;
        }

        public Event(Formula pre, IDictionary<Proposition, bool> post)
        {
            if (post != null)
            {
                IDictionary<ushort, bool> dict = new Dictionary<ushort, bool>();
                foreach (var entry in post)
                {
                    dict.Add(entry.Key.id, entry.Value);
                }
                this.post = dict;
            }
            else this.post = null;

            this.pre = pre;
            this.id = Counter;
            Counter++;
        }


        /// <summary>
        /// Sets the valuation of a proposition should have after applying the event
        /// </summary>
        /// <param name="proposition">The proposition to set.</param>
        /// <param name="value">Boolean value that the proposition is set to after event e is applied. Null means it will keep its previous value</param>
        public void SetPostcondition(Proposition prop, bool value)
        {
            this.SetPostcondition(prop.id, value);
        }

        /// <summary>
        /// Sets the valuation of a proposition should have after applying the event
        /// </summary>
        /// <param name="propositionId">The id of the proposition.</param>
        /// <param name="value">Boolean value that the proposition is set to after event e is applied. Null means it will keep its previous value</param>
        public void SetPostcondition(ushort propositionId, bool value)
        {
            try
            {
                this.post[propositionId] = value;
            }
            catch (KeyNotFoundException)
            {
                this.post.Add(propositionId, value);
            }
        }

        public bool IsEqualTo(IWorld other)
        {
            return this.TruePropositions == other.TruePropositions;
        }
    }
}