using System;
namespace ImplicitCoordination.DEL.utils
{
    public struct BitArray
    {
        public ulong data;

        public BitArray(ulong data = 0)
        {
            this.data = data;
        }

        public bool GetValue(ushort idx)
        {
            if (!(0 <= idx && idx <= 63))
            {
                throw new PropositionIdxOutOfRangeException("Cannot evaluate proposition.");
            }

            var value = (data >> idx) & 1;
            if (!(value == 0 || value == 1))
            {
                throw new Exception("Error while obtaining value from bitArray");
            }
            return value == 1;

        }

        public void SetValue(ushort idx, bool value)
        {
            // If we want to set the bit to true, simply OR the number with the shifted 1-bit
            if (value)
            {
                data |= Convert.ToUInt32((uint)1 << idx);
            }

            // If we want to set the bit, NOT the shifted 1-bit and AND it with the number
            else
            {
                data &= Convert.ToUInt32(~((uint)1 << idx));
            }
        }

    }
}
