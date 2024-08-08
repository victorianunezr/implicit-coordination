namespace ImplicitCoordination.PDDLParser.Models
{
    public class Object
    {
        public string Name { get; set; }
        public Type ObjectType { get; set; }

        public Object(string name, Type objectType)
        {
            Name = name;
            ObjectType = objectType;
        }
    }
}