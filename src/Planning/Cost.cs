using System;
namespace ImplicitCoordination.Planning
{
    public struct Cost
    {
        /// <summary>
        /// Cost numeric value. Infinity is ushort.MaxValue. Null is undefined.
        /// </summary>
        public ushort? value;
        /// <summary>
        /// Range is true if the cost is not a fixed value, but a range e.g. [1, \inf) or 1+
        /// </summary>
        public bool isRange;
    }
}
