using System.Collections.Generic;

namespace ImplicitCoordination.PDDLParser.Models
{
    public class ProblemModel
    {
        public List<Object> Objects { get; set; } = new List<Object>();
        public List<Predicate> InitialState { get; set; } = new List<Predicate>();
        public List<Predicate> GoalState { get; set; } = new List<Predicate>();
    }
}