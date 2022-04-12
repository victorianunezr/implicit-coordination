using System;
using System.Collections.Generic;
using System.Text;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.utils
{
    public static class HashingHelper
    {
        public static string CompressEntry(KeyValuePair<Agent, HashSet<(World, World)>> entry)
        {
            List<(ulong, ulong)> edges = new List<(ulong, ulong)>();
            foreach (var (w, u) in entry.Value)
            {
                edges.Add((w.valuation.data, u.valuation.data));
            }

            SortListOfTuples(edges);

            StringBuilder builder = new StringBuilder($"{entry.Key.Id}:");

            foreach (var (w, u) in edges)
            {
                builder.Append($"({w},{u})");
            }

            return builder.ToString();
        }

        public static void SortListOfTuples(List<(ulong, ulong)> list)
        {
            SortTuplesInList(list);

            list.Sort((t1, t2) =>
            {
                int res = t1.Item1.CompareTo(t2.Item1);
                return res != 0 ? res : t1.Item2.CompareTo(t2.Item2);
            });
        }

        public static void SortTuplesInList(List<(ulong, ulong)> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = SortTuple(list[i]);
            }
        }

        public static (ulong, ulong) SortTuple((ulong, ulong) tup)
        {
            var t1 = tup.Item1;
            var t2 = tup.Item2;

            if (t1 <= t2) return tup;
            else return (t2, t1);
        }
    }
}
