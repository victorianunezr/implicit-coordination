using System.Collections.Generic;

namespace ImplicitCoordination.PDDLParser.Models
{
    public class DomainModel
    {
        public List<Type> Types { get; set; } = new List<Type>();
        public List<Predicate> Predicates { get; set; } = new List<Predicate>();
        public List<Action> Actions { get; set; } = new List<Action>();
    }
}