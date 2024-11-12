using System;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// A proposition is a statement or assertion that can either be true or false
    /// </summary>
    public class Proposition
    {
        private static ushort Counter = 0;
        public string name;
        public ushort id; // id will be used to access truth assignment of proposition in bitvector of a world

        public Proposition(string name)
        {
            this.name = name;
            this.id = Counter;
            Counter++;
            // if (Counter >= 64)
            // {
            //     throw new PropositionIdxOutOfRangeException("No more space for new propositions.");
            // }
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }
    }


    public class PropositionIdxOutOfRangeException : Exception
    {
        public PropositionIdxOutOfRangeException(string msg) : base(msg) { }
    }
}