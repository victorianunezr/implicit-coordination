
namespace ImplicitCoordination.PDDLParser.Parsers
{
    using PDDLParser.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class ProblemParser : BaseParser
    {
        private Dictionary<string, Object> objectDictionary = new Dictionary<string, Object>();

        public ProblemModel ParseProblem(string filePath, DomainModel domainModel)
        {
            var problemModel = new ProblemModel();
            var lines = ReadFile(filePath);

            foreach (var line in lines)
            {
                if (line.Contains(":objects"))
                {
                    ParseObjects(line, problemModel);
                }
                else if (line.Contains(":init"))
                {
                    ParseInitialState(line, problemModel, domainModel);
                }
                else if (line.Contains(":goal"))
                {
                    ParseGoalState(line, problemModel, domainModel);
                }
            }

            return problemModel;
        }

        private void ParseObjects(string line, ProblemModel problemModel)
        {
            var objectData = ExtractData(line);
            foreach (var obj in objectData)
            {
                var parts = obj.Split(' ');
                var objectName = parts[0];
                var objectType = parts.Length > 2 ? parts[2] : null; // Optional: Handle types if provided

                var pddlObject = new Object(objectName, objectType != null ? new Type(objectType) : null);
                problemModel.Objects.Add(pddlObject);
                objectDictionary[objectName] = pddlObject; // Store in dictionary for quick lookup
            }
        }
        
        private void ParseInitialState(string line, ProblemModel problemModel, DomainModel domainModel)
        {
            var initData = ExtractData(line);
            foreach (var pred in initData)
            {
                var parts = pred.Split(' ');
                var predicateName = parts[0];

                // Find the corresponding predicate in the domain model
                var predicateTemplate = domainModel.Predicates.FirstOrDefault(p => p.Name == predicateName);
                if (predicateTemplate == null) continue;

                // Get the objects for the predicate
                var objects = new List<Object>();
                for (int i = 1; i < parts.Length; i++)
                {
                    if (objectDictionary.TryGetValue(parts[i], out var pddlObject))
                    {
                        objects.Add(pddlObject);
                    }
                }

                // Create a new predicate instance for the initial state with these objects
                var predicateInstance = new Predicate(predicateName, objects);
                problemModel.InitialState.Add(predicateInstance);
            }
        }

        private void ParseGoalState(string line, ProblemModel problemModel, DomainModel domainModel)
        {
            var goalData = ExtractData(line);
            foreach (var pred in goalData)
            {
                var parts = pred.Split(' ');
                var predicateName = parts[0];

                // Find the corresponding predicate in the domain model
                var predicateTemplate = domainModel.Predicates.FirstOrDefault(p => p.Name == predicateName);
                if (predicateTemplate == null) continue;

                // Get the objects for the predicate
                var objects = new List<Object>();
                for (int i = 1; i < parts.Length; i++)
                {
                    if (objectDictionary.TryGetValue(parts[i], out var pddlObject))
                    {
                        objects.Add(pddlObject);
                    }
                }

                // Create a new predicate instance for the goal state with these objects
                var predicateInstance = new Predicate(predicateName, objects);
                problemModel.GoalState.Add(predicateInstance);
            }
        }

        public override void Parse(string filePath, DomainModel domain)
        {
            ParseProblem(filePath, domain);
        }
    }
}
