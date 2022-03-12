using System;
using System.Collections.Generic;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    public class State
    {
        public HashSet<IWorld> possibleWorlds;

        public HashSet<IWorld> designatedWorlds;

        public AccessibilityRelation accessibility;

        public State(
            HashSet<IWorld> possibleWorlds,
            HashSet<IWorld> designatedWorlds,
            AccessibilityRelation accessibility)
        {
            this.possibleWorlds = possibleWorlds ?? throw new ArgumentNullException(nameof(possibleWorlds));

            if (designatedWorlds == null || designatedWorlds.IsSubsetOf(possibleWorlds))
            {
                this.designatedWorlds = designatedWorlds ?? new HashSet<IWorld>();
            }
            else
            {
                throw new ArgumentException("Set of designated worlds is not a subset of possible worlds.");
            }

            this.accessibility = accessibility ?? throw new ArgumentNullException(nameof(accessibility));
        }


        public State ProductUpdate(Action action)
        {
            HashSet<IWorld> newPossibleWorlds = new HashSet<IWorld>();
            HashSet<IWorld> newDesignatedWorlds = new HashSet<IWorld>();
            AccessibilityRelation newAccessibility = this.accessibility.CopyEmptyGraph();


            foreach (World w in this.possibleWorlds)
            {
                foreach (Event e in action.events)
                {
                    if (w.IsValid(this, e.pre))
                    {
                        // If precondition of e holds in w, create child world w'
                        World wPrime = w.CreateChild(e);

                        // Update valuation of w' according to postcondition of e. Valuation only changes if e.post != null
                        UpdateValuation(wPrime, e.post);

                        newPossibleWorlds.Add(wPrime);

                        if (this.designatedWorlds.Contains(w) && action.designatedEvents.Contains(e))
                        {
                            newDesignatedWorlds.Add(wPrime);
                        }
                    }
                }
            }

            UpdateAccessibility(action, newAccessibility, newPossibleWorlds);
            return new State(newPossibleWorlds, newDesignatedWorlds, newAccessibility);
        }

        public static void UpdateValuation(World w, IDictionary<ushort, bool?> post)
        {
            if (post != null)
            {
                foreach (var entry in post)
                {
                    if (entry.Value != null)
                    {
                        w.SetValuation(entry.Key, entry.Value == true);
                    }
                }
            }

        }

        public void UpdateAccessibility(Action action, AccessibilityRelation newAccessibility, HashSet<IWorld> newWorlds)
        {

            foreach (World w in newWorlds)
            {
                foreach (World v in newWorlds)
                {
                    foreach (Agent a in this.accessibility.graph.Keys)
                    {
                        try
                        {
                            if (this.accessibility.graph[a].ContainsEdge(w.parentWorld, v.parentWorld)
                                && action.accessibility.graph[a].ContainsEdge(w.parentEvent, v.parentEvent))
                            {
                                if (!newAccessibility.graph[a].ContainsEdge(w, v))
                                {
                                    newAccessibility.graph[a].Add((w, v));
                                }
                            }
                        }
                        catch (KeyNotFoundException) {  }
                    }
                }
            }
        }
    }
}