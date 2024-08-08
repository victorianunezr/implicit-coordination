using System.Collections.Generic;

namespace ImplicitCoordination.PDDLParser.Models
{
    public class Predicate
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }

        public Predicate(string name, List<string> parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}