
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class Problem
    {
        public string name { get; set; }
        public State initialState { get; set; }
        public Formula goalFormula { get; set; }
        public IEnumerable<Object> Objects { get; internal set; }
        private Dictionary<string, Object> objectsByName;
        public static HashSet<GroundPredicate> GroundPredicates { get; private set; } = new HashSet<GroundPredicate>();
        public static Dictionary<GroundPredicate, int> GroundPredicateToIndex { get; private set; } = new Dictionary<GroundPredicate, int>();
        public static List<GroundPredicate> IndexToGroundPredicate { get; private set; } = new List<GroundPredicate>();
        public static int TotalGroundPredicatesCount => GroundPredicateToIndex.Count;
        
        /// <summary>
        /// Generate and store all ground predicates by instantiating each domain-level predicate
        /// with objects in this problem that match parameter types.
        /// </summary>
        /// <param name="domain">The domain containing the predicate schemas.</param>
        public void PopulateGroundPredicates(Domain domain)
        {
            // 1. Group the problem's objects by type for quick lookup
            //    Key = object type (string), Value = list of objects of that type
            var objectsByType = Objects
                .GroupBy(o => o.Type)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 2. For each predicate schema in the domain
            foreach (var schema in domain.Predicates)
            {
                // Build a list-of-lists: each sub-list has all objects for the param's type
                var listsOfCandidates = new List<List<Object>>();
                bool skipPredicate = false;

                foreach (var param in schema.Parameters)
                {
                    if (objectsByType.TryGetValue(param.Type, out var candidateObjects))
                    {
                        listsOfCandidates.Add(candidateObjects);
                    }
                    else
                    {
                        // If there are no objects matching this parameter's type,
                        // then no instantiations can be formed.
                        skipPredicate = true;
                        break;
                    }
                }

                if (skipPredicate) 
                    continue;

                // 3. Compute the Cartesian product of these candidate object-lists
                var allCombinations = CartesianProduct(listsOfCandidates);
                
                // 4. For each combination, build a GroundPredicate
                foreach (var combo in allCombinations)
                {
                    GroundPredicates.Add(new GroundPredicate(schema.name, combo));
                }
            }

            CreateIndexesForGroundPredicates();
            BuildObjectLookup();
        }

        /// <summary>
        /// Returns the Cartesian product of a list of lists.
        /// Example: for [[A, B], [X, Y]] => we get [A,X], [A,Y], [B,X], [B,Y].
        /// </summary>
        private static IEnumerable<List<Object>> CartesianProduct(List<List<Object>> lists)
        {
            if (lists.Count == 0)
            {
                // If no parameters, yield an empty list
                yield return new List<Object>();
                yield break;
            }

            // Take the first set of candidates
            var firstList = lists[0];
            // Recursively compute Cartesian product of the remaining sets
            var remaining = lists.Skip(1).ToList();

            if (remaining.Count == 0)
            {
                // Base case: just yield each item as its own list
                foreach (var item in firstList)
                {
                    yield return new List<Object> { item };
                }
            }
            else
            {
                // For each item in the first set, pair it with all combinations from the rest
                foreach (var item in firstList)
                {
                    foreach (var restCombo in CartesianProduct(remaining))
                    {
                        var newCombo = new List<Object>(restCombo.Count + 1);
                        newCombo.Add(item);
                        newCombo.AddRange(restCombo);
                        yield return newCombo;
                    }
                }
            }
        }

        public void CreateIndexesForGroundPredicates()
        {
            GroundPredicateToIndex.Clear();
            IndexToGroundPredicate.Clear();

            var orderedPreds = GroundPredicates.OrderBy(gp => gp.Name)
                                            .ThenBy(gp => string.Join(",", gp.Arguments.Select(a => a.Name)))
                                            .ToList();

            // Assign indexes sequentially
            int currentIndex = 0;
            foreach (var gp in orderedPreds)
            {
                GroundPredicateToIndex[gp] = currentIndex;
                IndexToGroundPredicate.Add(gp);
                currentIndex++;
            }
        }

        public void BuildObjectLookup()
        {
            // Initialize or clear the dictionary
            objectsByName = new Dictionary<string, Object>();

            // Populate from the Objects collection
            foreach (var obj in Objects)
            {
                // If name collisions are possible, you might want to handle them here.
                objectsByName[obj.Name] = obj;
            }
        }

        public Object GetObjectByName(string name)
        {
            if (objectsByName == null)
            {
                throw new InvalidOperationException(
                    "objectsByName is null. Did you forget to call BuildObjectLookup() after assigning Objects?");
            }

            if (!objectsByName.TryGetValue(name, out var obj))
            {
                throw new Exception($"Object '{name}' not found in this Problem's object set.");
            }

            return obj;
        }
    }   
}