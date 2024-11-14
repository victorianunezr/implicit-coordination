using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Problem
    {
        private Domain domain;
        public List<ulong> PredicateTruthValues { get; set; }  // Each ulong stores 64 predicate truth values in bits

        public Problem(Domain domain)
        {
            this.domain = domain;
            int numPredicates = domain.Predicates.Count;
            PredicateTruthValues = new List<ulong>(new ulong[(numPredicates + 63) / 64]);
        }

        public bool GetPredicateValue(string predicateName)
        {
            int index = domain.PredicateIndices[predicateName];
            int ulongIndex = index / 64;
            int bitIndex = index % 64;
            return (PredicateTruthValues[ulongIndex] & (1UL << bitIndex)) != 0;
        }

        public void SetPredicateValue(string predicateName, bool value)
        {
            int index = domain.PredicateIndices[predicateName];
            int ulongIndex = index / 64;
            int bitIndex = index % 64;

            if (value)
                PredicateTruthValues[ulongIndex] |= (1UL << bitIndex);  // Set bit to 1
            else
                PredicateTruthValues[ulongIndex] &= ~(1UL << bitIndex); // Set bit to 0
        }
    }
}