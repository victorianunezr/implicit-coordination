using System;
namespace ImplicitCoordination.DEL
{
    public interface IWorld
    {
        public ushort Id { get; }

        public ulong TruePropositions { get; }

        public bool IsEqualTo(IWorld other);

        public void somemethod() { }

    }
}
