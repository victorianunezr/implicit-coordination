using ImplicitCoordination.DEL.utils;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Event : IWorld
    {
        /// <summary>
        /// Precondition Formula that is evaluated in each world when applying the product update.
        /// </summary>
        public Formula pre { get; set; }

        /// <summary>
        /// Postcondition is a dictionary mapping a proposition id to their valuation.
        /// </summary>
        /// <remarks>
        /// It is modelled as a dictionary to allow null values. When a proposition p maps to null,
        /// it means the valuation for p is not set, and if w |= p before applying the event,
        /// the resulting world w' after applying the event will have w' |= p.
        /// If a postcondition is a negated proposition ~p, set post[p.id] to false.
        /// If post is null, it is interpreted as all entries being null.
        /// </remarks>
        public IDictionary<ushort, bool?>? post;

        public Event(Formula pre, IDictionary<ushort, bool?> post=null)
        {
            this.pre = pre;
            this.post = post;
        }

        /// <summary>
        /// Sets the valuation of a proposition should have after applying the event
        /// </summary>
        /// <param name="propositionId">The id of the proposition.</param>
        /// <param name="value">Boolean value that the proposition is set to after event e is applied. Null means it will keep its previous value</param>
        public void SetPostcondition(ushort propositionId, bool? value)
        {
            this.post[propositionId] = value;
        }
    }
}