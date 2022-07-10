using System.Collections.Generic;
using ImplicitCoordination.DEL.utils;
using ImplicitCoordination.Planning;

namespace ImplicitCoordination.DEL
{
    public class World : IWorld
    {
        private static ushort Counter = 0;
        private readonly ushort id;
        public ushort Id => this.id;
        public HashSet<Proposition> TruePropositions = new HashSet<Proposition>();

        /// <summary>
        /// valuation[i] gives truth value for atomic proposition with id i. BitArray currently only allows 64 propositions per world.
        /// If more propositions are needed, valuation can be modified into a collection of BitArrays.
        /// </summary>
        public BitArray valuation;

        /// <summary>
        /// Incoming edge keeps track of parent world and event from which this new world was generated.
        /// </summary>
        public WorldEdge incomingEdge;

        /// <summary>
        /// Keeps track of child worlds, i.e. worlds generated from 'this' during product update
        /// </summary>
        public ICollection<WorldEdge> outgoingEdges = new HashSet<WorldEdge>();

        /// <summary>
        /// cost(w)
        /// </summary>
        public Cost cost;

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
        /// <param name="valuation"></param>
        /// <remarks>
        /// Least significant bit in valuation corresponds to proposition with id=0.
        /// Instantiating a World without passing a valuation results in a world with all values set to false.
        /// </remarks>
        public World(ulong valuationData = 0)
        {
            this.valuation = new BitArray(valuationData);
            this.id = Counter;
            Counter++;
        }

        public bool IsTrue(Proposition p)
        {
            return this.valuation.GetValue(p.id);
        }

        public void AddProposition(Proposition p)
        {
            this.SetValuation(p.id, true);
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
        public World CreateChild(Action a, Event e)
        {
            World childWorld = new World(this.valuation.data);
            WorldEdge edge = new WorldEdge(childWorld, this, e, a);
            this.outgoingEdges.Add(edge);
            childWorld.incomingEdge = edge;
            return childWorld;
        }

        /// <summary>
        /// Two worlds are equal if their valuation (set of true propositions) is equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if worlds valuations are equal</returns>
        public bool IsEqualTo(IWorld other)
        {
            return this.TruePropositions == other.TruePropositions;
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }

        public bool HasAnyApplicableEvent(Action action, State s)
        {
            foreach (Event e in action.possibleWorlds)
            {
                if (e.pre.Evaluate(s, this)) return true;
            }

            return false;
        }
    }
}