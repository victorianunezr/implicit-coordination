using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class State
    {
        public HashSet<World> possibleWorlds;

        public HashSet<World> designatedWorlds;

        public AccessibilityRelation accessibility;

        public State(
            HashSet<World> possibleWorlds,
            HashSet<World> designatedWorlds,
            AccessibilityRelation accessibility)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));

            if (!(designatedWorlds == null || designatedWorlds.IsSubsetOf(possibleWorlds)))
            {
                this.designatedWorlds = designatedWorlds ?? new HashSet<World>();
            }
            else
            {
                throw new ArgumentException("Set of designated worlds is not a subset of possible worlds.");
            }

            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }

        public State ProductUpdate(Action action)
        {
            HashSet<(World, Event)> newPossibleWorlds = new HashSet<(World, Event)>();
            HashSet<(World, Event)> newDesignatedWorlds = new HashSet<(World, Event)>();


            foreach (World w in this.possibleWorlds)
            {
                foreach (Event e in action.events)
                {
                    if (w.IsValid(this, e.pre))
                    {
                        // If precondition of e holds in w, copy world into w'
                        World wPrime = w.Copy();

                        // Update valuation of w' according to postcondition of e. Valuation only changes if e.post != null
                        UpdateValuation(wPrime, e.post);

                        newPossibleWorlds.Add((wPrime, e));

                        if (this.possibleWorlds.Contains(w) && action.designatedEvents.Contains(e))
                        {
                            newDesignatedWorlds.Add((w, e));
                        }
                    }

                }
            }
        }

        private static void UpdateValuation(World w, IDictionary<ushort, bool?> post)
        {
            if (post != null)
            {
                // TODO: is there a more efficient way to update the valuation other than iterating through all 64 possible propositions?
                for (ushort i = 0; i < 64; i++)
                {
                    if (post[i] == true)
                    {
                        w.SetValuation(i, true);
                    }
                    else if (post[i] == false)
                    {
                        w.SetValuation(i, false);
                    }
                }
            }

        }
    }
}