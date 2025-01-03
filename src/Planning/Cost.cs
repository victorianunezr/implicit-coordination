using System;
namespace ImplicitCoordination.Planning
{
    public readonly struct Cost : IComparable<Cost>
    {
        public CostType Type { get; }
        public int Value { get; }

        private Cost(CostType type, int value = 0)
        {
            Type = type;
            Value = value;
        }

        public static Cost Finite(int value)
        {
            if (value < 0)
                throw new ArgumentException("Finite cost must be a positive integer.");
            return new Cost(CostType.Finite, value);
        }

        public static Cost Infinity() => new Cost(CostType.Infinity);
        public static Cost Undefined() => new Cost(CostType.Undefined);

        public static bool operator <(Cost left, Cost right) => left.CompareTo(right) < 0;
        public static bool operator >(Cost left, Cost right) => left.CompareTo(right) > 0;
        public static bool operator <=(Cost left, Cost right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Cost left, Cost right) => left.CompareTo(right) >= 0;

        public int CompareTo(Cost other)
        {
            if (Type == other.Type)
            {
                if (Type == CostType.Finite)
                {
                    return Value.CompareTo(other.Value);
                }
                return 0;
            }

            return Type switch
            {
                CostType.Finite => -1,
                CostType.Undefined => other.Type == CostType.Infinity ? -1 : 1,
                CostType.Infinity => 1,
                _ => throw new InvalidOperationException("Unexpected CostType.")
            };
        }

        public override string ToString()
        {
            return Type switch
            {
                CostType.Finite => Value.ToString(),
                CostType.Infinity => "Infinity",
                CostType.Undefined => "+",
                _ => "Unknown"
            };
        }
    }

    public enum CostType
    {
        Unassigned,
        Finite,
        Infinity,
        Undefined
    }
}
