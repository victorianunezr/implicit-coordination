using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
    {
    public class GroundPredicate
    {
        public string Name { get; }
        public List<Object> Arguments { get; } = new();

        public GroundPredicate(string name, IEnumerable<Object> args)
        {
            Name = name;
            Arguments.AddRange(args);
        }

        public override string ToString()
        {
            // e.g.: "at(Alice, x)"
            var argsList = string.Join(", ", Arguments.Select(a => a.Name));
            return $"{Name}({argsList})";
        }

        // Equality is based on Predicate's name and arguments' names, but ignores arguments' types.
        public override bool Equals(object obj)
        {
            if (obj is GroundPredicate other)
            {
                if (!Name.Equals(other.Name)) return false;
                if (Arguments.Count != other.Arguments.Count) return false;
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Name.Equals(other.Arguments[i].Name)) return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Name.GetHashCode();
                foreach (var arg in Arguments)
                {
                    // Use the argument's name hash code rather than the object's hash code.
                    hash = hash * 31 + (arg.Name?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}