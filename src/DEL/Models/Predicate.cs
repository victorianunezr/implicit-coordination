using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool isNegated;
        public List<Parameter> Parameters { get; set; }

        public Predicate(){}
        
        public Predicate(string name, List<Parameter> parameters=null, bool isNegated=false)
        {
            this.name = name;
            this.id = Counter;
            this.isNegated = isNegated;
            Counter++;
            Parameters = parameters ?? new List<Parameter>();
        }

        public Predicate(string name, List<string> parameterNames, bool isNegated=false)
        {
            this.name = name;
            this.id = Counter;
            this.isNegated = isNegated;
            Counter++;
            Parameters = new List<Parameter>();
            foreach (string param in parameterNames)
            {
                Parameters.Add(new Parameter(param, "default"));
            }
        }

        public Predicate WithParameters(List<Parameter> parameters)
        {
            return new Predicate(name, parameters, isNegated);
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Predicate other)
                return false;

            return string.Equals(name, other.name, StringComparison.Ordinal) &&
                   Parameters.Count == other.Parameters.Count &&
                   !Parameters.Where((t, i) => !t.Name.Equals(other.Parameters[i].Name, StringComparison.Ordinal)).Any();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = name.GetHashCode();
                foreach (var parameter in Parameters)
                {
                    hash = (hash * 397) ^ parameter.Name.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            var paramsStr = string.Join(", ", Parameters.Select(p => p.Name));
            return $"{(isNegated ? "~" : "")}{name}({paramsStr})";
        }
    }
}