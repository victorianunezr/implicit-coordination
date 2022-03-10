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
    }
}
