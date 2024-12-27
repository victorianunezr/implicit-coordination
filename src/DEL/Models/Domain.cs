using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Domain
    {
        public string name;
        public HashSet<Action> actions { get; set; } = new();
        public Dictionary<string, int> PredicateIndices { get; set; } = new();
        public HashSet<Predicate> Predicates { get; set; } = new();

        public int AddPredicate(Predicate predicate)
        {
            if (!PredicateIndices.ContainsKey(predicate.name))
            {
                PredicateIndices[predicate.name] = Predicates.Count;
                Predicates.Add(predicate);
            }
            return PredicateIndices[predicate.name];
        }
    }
}
