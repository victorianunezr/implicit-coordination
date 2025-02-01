namespace ImplicitCoordination.DEL
{
    public abstract class NamedEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }

        protected NamedEntity(string name, string type)
        {
            Name = name;
            Type = type;
        }
        public override string ToString()
        {
            return Type != null ? $"{Name}:{Type}" : Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is not NamedEntity other)
                return false;

            bool sameName = string.Equals(this.Name, other.Name, System.StringComparison.OrdinalIgnoreCase);
            bool sameType = string.Equals(this.Type, other.Type, System.StringComparison.OrdinalIgnoreCase);

            return sameName && sameType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Name?.ToLowerInvariant().GetHashCode() ?? 0);
                hash = hash * 31 + (Type?.ToLowerInvariant().GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    public class Object : NamedEntity
    {
        public Object(string name, string type) : base(name, type) { }
    }

    public class Parameter : NamedEntity
    {
        public Parameter(string name, string type) : base(name, type) { }
    }
}