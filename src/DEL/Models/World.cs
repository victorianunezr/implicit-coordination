using System;
using System.Collections;
using System.Collections.Generic;
using ImplicitCoordination.Planning;

namespace ImplicitCoordination.DEL
{
    public class World : IWorld
    {
        private static ushort Counter = 0;
        private readonly ushort id;
        public static Problem Problem;
        public ushort Id => this.id;
        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value != null) this.name = value;
                else this.name = $"w{id}";
            }
        }
        
        //todo: deprecate. Use _facts instead.
        public HashSet<Predicate> predicates = new HashSet<Predicate>();
            
        /// <summary>   
        // A BitArray where each bit corresponds to one ground predicate.
        // If Facts[index] = true, then that ground predicate is "true" in this state.
        /// </summary>
        public BitArray Facts;

        /// <summary>
        /// Incoming edge keeps track of parent world and event from which this new world was generated.
        /// </summary>
        public WorldEdge incomingEdge;

        /// <summary>
        /// Keeps track of child worlds, i.e. worlds generated from 'this' during product update
        /// </summary>
        public ICollection<WorldEdge> outgoingEdges = new HashSet<WorldEdge>();

        /// <summary>
        /// c_o(w)
        /// </summary>
        public Cost objectiveCost;

        /// <summary>
        /// c_s(w)
        /// </summary>
        public Cost subjectiveCost;

        /// <summary>
        /// cost(w,i)
        /// </summary>
        public IDictionary<Agent, Cost> worldAgentCost = new Dictionary<Agent, Cost>();

        /// <summary>
        /// True if the world was pruned by forward induction.
        /// </summary>
        public bool isPruned;

        /// <summary>
        /// Returns a world with automatically incremented id and BitArray with proposition valuations.
        /// </summary>
        /// <param name="totalGroundPredicatesCount">The total number of ground predicates in the problem.</param>
        /// <remarks>
        /// Least significant bit in valuation corresponds to proposition with id=0.
        /// </remarks>
        public World(int totalGroundPredicatesCount=32)
        {
            Facts = new BitArray(totalGroundPredicatesCount, false);
            this.id = Counter;
            Counter++;
        }

        public World(string name, int totalGroundPredicatesCount=32)
        {
            Name = name;
            Facts = new BitArray(totalGroundPredicatesCount, false);
            this.id = Counter;
            Counter++;
        }

        public World(World other)
        {
            Facts = new BitArray(other.Facts);
            Name = name;
            this.id = Counter;
            Counter++;
        }

        public bool IsTrue(Predicate p)
        {
            return this.predicates.Contains(p);
        }

        public void AddPredicate(Predicate p)
        {
            this.predicates.Add(p);
        }

        public void SetValuation(Predicate p, bool value)
        {
            if (value)
            {
                this.predicates.Add(p);
            }
            else
            {
                this.predicates.Remove(p);
            }
        }

        public bool IsGroundPredicateValid(State s, Formula f)
        {
            return f.Evaluate(s, this);
        }

        public EvaluationResult IsValid(State s, Formula f)
        {
            return f.EvaluateSchematic(s, this, Problem.Objects);
        }

        //todo: deprecate. Use constructor instead
        public World Copy()
        {
            World w = new World(this.Facts.Count);
            if (w.predicates != null)
            {
                w.predicates = new HashSet<Predicate>(this.predicates);
            }
            return w;
        }

        /// <summary>
        /// Creates a child world with the provided world and event as parents.
        /// The child world is initialized with the parent world's valuation data.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public World CreateChild(Action action, Event evt, Agent actionOwner=null)
        {
            World childWorld = new World(this);
            WorldEdge edge = new WorldEdge(childWorld, this, evt, action, actionOwner);
            this.outgoingEdges.Add(edge);
            childWorld.incomingEdge = edge;
            return childWorld;
        }

        /// <summary>
        /// Two worlds are equal if their valuation (set of true propositions) is equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if worlds valuations are equal</returns>
        public bool IsEqualTo(World other)
        {
            return this.predicates.SetEquals(other.predicates);
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }

        public bool IsApplicable(State state, Action action)
        {
            foreach (Event e in action.designatedWorlds)
            {
                if (e.pre.EvaluateSchematic(state, this, Problem.Objects).Satisfied) return true;
            }
            return false;
        }

        public bool HasAnyApplicableEvent(ICollection<Action> actions, State s)
        {
            foreach (Action a in actions)
            {
                foreach (Event e in a.possibleWorlds)
                {
                    if (e.pre.EvaluateSchematic(s, this, Problem.Objects).Satisfied) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Mark the bit at the given index as true (set the predicate as true in this state).
        /// </summary>
        public void SetFactTrue(int index)
        {
            Facts.Set(index, true);
        }

        /// <summary>
        /// Mark the bit at the given index as false (set the predicate as false in this state).
        /// </summary>
        public void SetFactFalse(int index)
        {
            Facts.Set(index, false);
        }

        /// <summary>
        /// Check if the bit at the given index is true (meaning the ground predicate is true in this state).
        /// </summary>
        public bool IsFactTrue(int index)
        {
            return Facts.Get(index);
        }

        /// <summary>
        /// Helper method to check if a particular GroundPredicate is true,
        /// given we have a Problem that can map the GroundPredicate to an index.
        /// </summary>
        public bool IsTrue(GroundPredicate gp)
        {
            if (!Problem.GroundPredicateToIndex.TryGetValue(gp, out int idx))
                return false;  // This ground predicate doesn't exist in the problem indexing
            return Facts.Get(idx);
        }

        public void PrintFacts()
        {
            Console.WriteLine("Facts for world " + Name + ":");
            for (int i = 0; i < Facts.Length; i++)
            {
                if (Facts.Get(i))
                {
                    Console.WriteLine(Problem.IndexToGroundPredicate[i].ToString());
                }
            }
        }
    }
}