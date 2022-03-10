using System;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// Proposition are atomic n-ary formulas and their negations, e.g. Has(Alice, Keys), ~Has(Bob, Keys)
    /// </summary>
    public class Proposition
    {
        private static ushort Counter = 0;
        public string name;
        public ushort id; // id will be used to access truth assignment of proposition in bitvector of a world
        public ushort arity;

        public Proposition(string name, int arity=0)
        {
            this.name = name;
            this.id = Counter;
            Counter++;
            if (Counter >= 64)
            {
                throw new PropositionIdxOutOfRangeException("No more space for new propositions.");
            }
        }
    }

    public class PropositionIdxOutOfRangeException : Exception
    {
        public PropositionIdxOutOfRangeException(string msg) : base(msg) { }
    }
}