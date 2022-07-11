using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool ContainsSameWorlds(this HashSet<World> thisSet, HashSet<World> other)
        {
            if (thisSet.Count != other.Count) return false;
            foreach (World w in thisSet)
            {
                if (!other.Any(x => x.IsEqualTo(w))) return false;
            }
            return true;
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
