using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public interface IWorld
    {
        public ushort Id { get; }
        public string name { get; set; }

        // public HashSet<Proposition> TruePropositions { get; }

        //public bool IsEqualTo(IWorld other);

    }
}
