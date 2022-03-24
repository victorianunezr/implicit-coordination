using System;
using System.Collections.Generic;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    public class State
    {
        //todo: implement LocalState
        //todo: maybe create an interface for epistemic model from which both State and Action inherit.
        public HashSet<IWorld> possibleWorlds;

        public HashSet<IWorld> designatedWorlds;

        public AccessibilityRelation accessibility;

        /// <summary>
        /// Pointer to global state from which local (or perspective shifted state) is generated.
        /// For higher order shifted states, e.g. (s^i)^j, it is a pointer to the state of one order less, e.g. s^i
        /// </summary>
        public State globalState;

        public State(
            HashSet<IWorld> possibleWorlds,
            HashSet<IWorld> designatedWorlds,
            AccessibilityRelation accessibility,
            State globalState=null)
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
            this.globalState = globalState;
        }

        public State(HashSet<IWorld> possibleWorlds, HashSet<IWorld> designatedWorlds, ICollection<Agent> agents)
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

            if (agents == null) throw new ArgumentNullException(nameof(agents));

            AccessibilityRelation acs = new AccessibilityRelation(agents, possibleWorlds);
            this.accessibility = acs;
        }


        /// <summary>
        /// Returns true if the state is in the set of goal states defined for the problem
        /// </summary>
        /// <returns></returns>
        public bool IsGoalState(Formula goalFormula)
        {
            return goalFormula.Evaluate(this);
        }


        /// <summary>
        /// Generates the perspective shift of s for agent a by closing on accessibility relation for a on the designated worlds of s.
        /// </summary>
        /// <returns>Returns the local state of s for agent a, i.e. s^a.</returns>
        public State PerspectiveShift(Agent a)
        {
            HashSet<IWorld> newDesignatedWorlds = new HashSet<IWorld>();

            foreach (IWorld w in this.designatedWorlds)
            {
                newDesignatedWorlds.UnionWith(this.accessibility.GetAccessibleWorlds(a, w));
            }

            return new State(this.possibleWorlds, newDesignatedWorlds, this.accessibility, this);
        }


        /// <summary>
        /// An action is applicable in a state if for all designated worlds there is a designated event s.t. (M,w) |= pre(e)
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Returns true if for all designated worlds there exists a designated events where precond holds.</returns>
        public bool IsApplicable(Action action)
        {
            bool eventExistsForWorld;

            foreach (World w in this.designatedWorlds)
            {
                eventExistsForWorld = false;

                foreach (Event e in action.designatedEvents)
                {
                    if (w.IsValid(this, e.pre))
                    {
                        eventExistsForWorld = true;
                        break;
                    }
                }

                if (!eventExistsForWorld)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Generates the new state s' resulting from applying action a on state s
        /// </summary>
        /// <param name="action">Action to apply.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Updates the valuation function of w according to the postcondition of the event e.
        /// </summary>
        /// <param name="w">New world in W' of which valuation function is updated</param>
        /// <param name="postcondition">Postcondition of e.</param>
        /// <remarks>
        /// The valuation of w is initalized as a copy of the parent world's valuation.
        /// This means that the new valuation of p will remain the same if it is not set to a value in the postcondition of e.
        /// </remarks>
        public static void UpdateValuation(World w, IDictionary<ushort, bool?> postcondition)
        {
            if (postcondition != null)
            {
                foreach (var entry in postcondition)
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