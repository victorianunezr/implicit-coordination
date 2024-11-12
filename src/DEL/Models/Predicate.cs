using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// Predicate are atomic n-ary formulas and their negations, e.g. Has(Alice, Keys), ~Has(Bob, Keys)
    /// </summary>
    public class Predicate
    {
        private static ushort Counter = 0;
        public string name;
        public ushort id; // id will be used to access truth assignment of predicate in bitvector of a world
        public List<Parameter> Parameters { get; set; }

        public Predicate(string name, List<Parameter> parameters)
        {
            this.name = name;
            this.id = Counter;
            Parameters = parameters;
            Counter++;
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}