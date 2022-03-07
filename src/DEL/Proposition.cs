using System;

namespace ImplicitCoordination.DEL
{
    public class Proposition
    {
        private static ushort Counter = 0;
        public string name;
        public ushort id; // id will be used to access truth assignment of proposition in bitvector of a world
        public ushort arity;

        public Proposition(string name, int arity)
        {
            this.name = name;
            this.id = Counter;
            Counter++;
            if (Counter >= 32)
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