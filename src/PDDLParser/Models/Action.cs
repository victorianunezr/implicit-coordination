using System.Collections.Generic;

namespace ImplicitCoordination.PDDLParser.Models
{
    public class Action
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public string Precondition { get; set; }
        public string Effect { get; set; }

        public Action(string name, List<string> parameters, string precondition, string effect)
        {
            Name = name;
            Parameters = parameters;
            Precondition = precondition;
            Effect = effect;
        }
    }
}