using System;
using System.Collections.Generic;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    public class State : EpistemicModel
    {
        //todo: consider implementing LocalState and GlobalState

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
            : base(possibleWorlds, designatedWorlds, accessibility)
        {
            this.globalState = globalState;
        }

        public State(HashSet<IWorld> possibleWorlds, HashSet<IWorld> designatedWorlds, ICollection<Agent> agents)
            : base(possibleWorlds, designatedWorlds, agents)
        {
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
        /// Generates the associated local state for a GLOBAL state for agent a by closing on accessibility relation for a on the designated world of s.
        /// </summary>
        /// <returns>Returns the local state of s for agent a, i.e. s^a.</returns>
        /// <remarks>If the state is not global</remarks>>
        public State GetAssociatedLocal(Agent a)
        {
            if (this.designatedWorlds.Count != 1)
            {
                throw new Exception($"The given state is not a global state. It contains {this.designatedWorlds.Count} designated worlds");
            }
            return this.PerspectiveShift(a);
        }


        //todo: do connected components search. Right now returned states are not minimal
        /// <summary>
        /// Generates the perspective shift of s for agent a by closing on accessibility relation for a on the designated worlds of s.
        /// </summary>
        /// <returns>Returns the perspective shifted state of s for agent a, i.e. s^a.</returns>
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
        /// Generator yielding the global states for a given states,
        /// i.e. returns the set of states {(M, w) | w \in W_d}, where W_d is the designated worlds of the input state.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<State> Globals()
        {
            foreach (IWorld w in this.designatedWorlds)
            {
                yield return new State(this.possibleWorlds, new HashSet<IWorld>() { w }, this.accessibility);
            }
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

                foreach (Event e in action.designatedWorlds)
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
        /// <returns>New state after appliying an action on the source state.</returns>
        public State ProductUpdate(Action action)
        {
            HashSet<IWorld> newPossibleWorlds = new HashSet<IWorld>();
            HashSet<IWorld> newDesignatedWorlds = new HashSet<IWorld>();
            AccessibilityRelation newAccessibility = this.accessibility.CopyEmptyGraph();

            bool eventExistsForWorld = false;

            foreach (World w in this.possibleWorlds)
            {
                if (this.designatedWorlds.Contains(w))
                {
                    eventExistsForWorld = false;
                }

                foreach (Event e in action.possibleWorlds)
                {
                    if (w.IsValid(this, e.pre))
                    {
                        // If precondition of e holds in w, create child world w'
                        World wPrime = w.CreateChild(e);

                        // Update valuation of w' according to postcondition of e. Valuation only changes if e.post != null
                        UpdateValuation(wPrime, e.post);

                        newPossibleWorlds.Add(wPrime);

                        if (this.designatedWorlds.Contains(w) && action.designatedWorlds.Contains(e))
                        {
                            newDesignatedWorlds.Add(wPrime);

                            eventExistsForWorld = true;
                        }
                    }
                }

                if (!eventExistsForWorld)
                {
                    // If we get here, it means we didn't find a designated world w and designated event e such that w |= pre(e)
                    // i.e. action is not applicable
                    return null;
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
        public static void UpdateValuation(World w, IDictionary<ushort, bool> postcondition)
        {
            if (postcondition != null)
            {
                foreach (var entry in postcondition)
                {
                    w.SetValuation(entry.Key, entry.Value);
                }
            }
        }


        /// <summary>
        /// Takes in a copy of the source accessibility relation and applies the changes according to the product update on it
        /// </summary>
        /// <param name="action"></param>
        /// <param name="newAccessibility"></param>
        /// <param name="newWorlds"></param>
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