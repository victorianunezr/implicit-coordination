using System;
namespace ImplicitCoordination.DEL
{
    public interface IWorld
    {
        public ushort Id { get; }

        public bool IsEqualTo(World other);

    }
}
