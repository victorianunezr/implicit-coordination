using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.utils
{
    public static class HashSetExtension
    {
        public static bool ContainsEdge(this HashSet<(IWorld, IWorld)> set, IWorld w, IWorld v)
        {
            return set.Contains((w, v)) || set.Contains((v, w));
        }

        public static bool ContainsEdge(this HashSet<(IWorld, IWorld)> set, (IWorld, IWorld) edge)
        {
            (IWorld w, IWorld v) = edge;
            return set.Contains((w, v)) || set.Contains((v, w));
        }

        public static T GetSingleElement<T>(this HashSet<T> set)
        {
            var iterator = set.GetEnumerator();

            if (!iterator.MoveNext())
            {
                throw new Exception("Set is empty");
            }

            T element = iterator.Current;

            if (iterator.MoveNext())
            {
                throw new Exception("Set contains more than one element");
            }
            return element;
        }
    }
}
