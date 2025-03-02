using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    public class State : EpistemicModel
    {
        private static ushort Counter = 1;
        private readonly ushort id;
        public static Problem Problem;
        public ushort Id => this.id;
        public string Name => $"s{id}";
        public State Parent { get; set; }
        public List<State> Children { get; set; } = new();     
        public string ActionName { get; set; }
        public ushort depth { get; set; }
        public State(): base() {}

        public State(
            HashSet<IWorld> possibleWorlds,
            HashSet<IWorld> designatedWorlds,
            AccessibilityRelation accessibility,
            State globalState=null)
            : base(possibleWorlds, designatedWorlds, accessibility)
        {
            id = Counter;
            Counter++;
        }

        public State(
            HashSet<IWorld> possibleWorlds,
            HashSet<IWorld> designatedWorlds,
            AccessibilityRelation accessibility,
            State parent,
            string actionName)
            : base(possibleWorlds, designatedWorlds, accessibility)
        {
            Parent = parent;
            depth = parent.depth;
            depth++;
            ActionName = actionName;
            id = Counter;
            Counter++;
        }

        public State(HashSet<IWorld> possibleWorlds, HashSet<IWorld> designatedWorlds, ICollection<Agent> agents)
            : base(possibleWorlds, designatedWorlds, agents)
        {
            id = Counter;
            Counter++;
        }

        /// <summary>
        /// Returns true if at least one world satisfies the goal formula
        /// </summary>
        /// <returns></returns>
        public bool HasGoalWorld(Formula goalFormula)
        {
            foreach (IWorld world in this.possibleWorlds)
            {
                if (goalFormula.Evaluate(this, (World)world)) { return true; }
            }
            return false;
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

            return new State(this.possibleWorlds, this.accessibility.GetAccessibleWorlds(a, this.designatedWorlds.GetSingleElement()), this.accessibility, this);
        }


        /// <summary>
        /// Generates the set of perspective shifted states of s for agent a by closing on accessibility relation for a on the designated worlds of s.
        /// </summary>
        /// <returns>Returns the set of perspective shifted states of s for agent a, i.e. s^a.</returns>
        public HashSet<State> PerspectiveShift(Agent a)
        {
            HashSet<IWorld> newDesignatedWorlds;
            HashSet<IWorld> seenDesignatedWorlds = new HashSet<IWorld>();
            HashSet<State> perspectiveShiftedStates = new HashSet<State>();

            foreach (IWorld w in this.designatedWorlds)
            {
                if (!seenDesignatedWorlds.Contains(w))
                {
                    newDesignatedWorlds = new HashSet<IWorld>();

                    foreach (IWorld reachableWorld in this.accessibility.GetAccessibleWorlds(a, w))
                    {
                        seenDesignatedWorlds.Add(reachableWorld);
                        newDesignatedWorlds.Add(reachableWorld);
                    }
                    perspectiveShiftedStates.Add(new State(this.possibleWorlds, newDesignatedWorlds, this.accessibility));
                }
            }

            return perspectiveShiftedStates;
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
        /// Generator yielding the global states for a given states,
        /// i.e. returns the set of states {(M, w) | w \in W_d}, where W_d is the designated worlds of the input state.
        /// </summary>
        /// <returns></returns>
        public HashSet<State> GetSetOfGlobals()
        {
            HashSet<State> globals = new HashSet<State>();
            foreach (IWorld w in this.designatedWorlds)
            {
                globals.Add(new State(this.possibleWorlds, new HashSet<IWorld>() { w }, this.accessibility));
            }
            return globals;
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
                    if (w.IsValid(this, e.pre).Satisfied)
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
        /// Designated worlds in the resulting state will be selected based on the set of designated worlds, if passed.
        /// Otherwise the W_d from the original state are used.</param>
        /// <returns>New state after appliying an action on the source state.</returns>
        public State ProductUpdate(Action action)
        {
            HashSet<IWorld> newPossibleWorlds = new HashSet<IWorld>();
            HashSet<IWorld> newDesignatedWorlds = new HashSet<IWorld>();

            foreach (World w in this.possibleWorlds)
            {
                foreach (Event e in action.possibleWorlds)
                {
                    var evalResult = w.IsValid(this, e.pre);

                    if (evalResult.Satisfied)
                    {
                        // If precondition of e holds in w, create child world w'
                        Agent actingAgent = GetActingAgent(evalResult.Assignment);
                        World wPrime = w.CreateChild(action, e, actingAgent);

                        // Update valuation of w' according to postcondition of e. Valuation only changes if e.post != null
                        UpdateValuation(wPrime, e.effect, evalResult.Assignment);

                        newPossibleWorlds.Add(wPrime);

                        if (this.designatedWorlds.Contains(w) && action.designatedWorlds.Contains(e))
                        {
                            newDesignatedWorlds.Add(wPrime);
                        }
                    }
                }
            }

            if (!newDesignatedWorlds.Any())
            {
                // If we get here, it means we didn't find a designated world w and designated event e such that w |= pre(e)
                // i.e. action is not applicable
                return null;
            }

            AccessibilityRelation newAccessibility = this.accessibility.CopyEmptyGraph(newPossibleWorlds);

            UpdateAccessibility(action, newAccessibility, newPossibleWorlds);
            return new State(newPossibleWorlds, newDesignatedWorlds, newAccessibility, this, action.name);
        }


        // UpdateValuation applies the effect of an event to a new world wPrime.
        // 'effect' is a dictionary mapping schematic Predicates to their desired truth value.
        // 'assignment' is a mapping from variable names to Objects, produced by the action instantiation.
        public static void UpdateValuation(World wPrime, IDictionary<Predicate, bool> effect, Dictionary<string, Object> assignment)
        {
            if (effect != null)
            {
                foreach (var kvp in effect)
                {
                    GroundPredicate gp = SubstituteVariables(kvp.Key, assignment);
                    if (Problem.GroundPredicateToIndex.TryGetValue(gp, out int idx))
                    {
                        wPrime.Facts.Set(idx, kvp.Value);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Ground predicate {gp} not found in index.");
                    }
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
                            if (this.accessibility.graph[a].ContainsEdge(w.incomingEdge.parentWorld, v.incomingEdge.parentWorld)
                                && action.accessibility.graph[a].ContainsEdge(w.incomingEdge.parentEvent, v.incomingEdge.parentEvent))
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

        private static GroundPredicate SubstituteVariables(Predicate p, Dictionary<string, Object> assignment)
        {
            var newArgs = new List<Object>();
            foreach (var param in p.Parameters)
            {
                if (param.Name.StartsWith("?"))
                {
                    if (assignment.TryGetValue(param.Name, out Object obj))
                        newArgs.Add(obj);
                    else
                        newArgs.Add(new Object(param.Name, param.Type));
                }
                else
                {
                    newArgs.Add(new Object(param.Name, param.Type));
                }
            }
            return new GroundPredicate(p.name, newArgs);
        }

        public Agent GetActingAgent(Dictionary<string, Object> assignment)
        {
            if (assignment.TryGetValue("?agent", out Object obj))
            {
                return new Agent(obj.Name);
            }
            throw new Exception("No acting agent found in the assignment.");
        }
    }
}