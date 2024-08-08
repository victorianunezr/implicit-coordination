namespace ImplicitCoordination.PDDLParser.Models
{
    public class Type
    {
        public string Name { get; set; }
        public Type ParentType { get; set; } // For type hierarchies, if needed

        public Type(string name, Type parentType = null)
        {
            Name = name;
            ParentType = parentType;
        }
    }
}