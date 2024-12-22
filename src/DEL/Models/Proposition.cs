using System;
namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// A proposition is a statement or assertion that can either be true or false
    /// </summary>
    [Obsolete]
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