using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public interface IWorld
    {
        public ushort Id { get; }

        // public HashSet<Proposition> TruePropositions { get; }

        public bool IsEqualTo(IWorld other);

    }
}
