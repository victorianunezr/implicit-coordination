using System.Collections.Generic;

namespace ImplicitCoordination.PDDLParser.Models
{
    public class Predicate
    {
        public string Name { get; set; }
        public List<(Object Object, Type ObjectType)> Parameters { get; set; }
        public int Arity => Parameters.Count;

        public Predicate(string name, List<(Object, Type)> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public bool ValidateTypes(List<Type> expectedTypes)
        {
            if (Parameters.Count != expectedTypes.Count)
                return false;

            for (int i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i].ObjectType != expectedTypes[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}