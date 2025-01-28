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