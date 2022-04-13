using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.utils
{
    public static class HashingHelper
    {
        private static readonly MD5 md5Hasher = MD5.Create();

        public static byte[] HashAccessibilityRelation(this AccessibilityRelation R)
        {
            return md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(R.AccessibilityGraphToString()));
        }

        public static string AccessibilityGraphToString(this AccessibilityRelation R)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var entry in R.graph)
            {
                builder.Append(KVPairToString(entry));
            }
            return builder.ToString();
        }

        public static string KVPairToString(KeyValuePair<Agent, HashSet<(IWorld, IWorld)>> entry)
        {
            List<(ulong, ulong)> edges = new List<(ulong, ulong)>();
            foreach (var (w, u) in entry.Value)
            {
                if (w.TruePropositions <= u.TruePropositions)
                {
                    edges.Add((w.TruePropositions, u.TruePropositions));
                }
                else
                {
                    edges.Add((u.TruePropositions, w.TruePropositions));
                }
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
            //SortTuplesInList(list);

            list.Sort((t1, t2) =>
            {
                int res = t1.Item1.CompareTo(t2.Item1);
                return res != 0 ? res : t1.Item2.CompareTo(t2.Item2);
            });
        }

        //public static void SortTuplesInList(List<(ulong, ulong)> list)
        //{
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        list[i] = SortTuple(list[i]);
        //    }
        //}

        //public static (ulong, ulong) SortTuple((ulong, ulong) tup)
        //{
        //    var t1 = tup.Item1;
        //    var t2 = tup.Item2;

        //    if (t1 <= t2) return tup;
        //    else return (t2, t1);
        //}
    }
}
