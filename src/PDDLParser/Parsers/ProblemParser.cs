
namespace ImplicitCoordination.PDDLParser.Parsers
{
    using System.Linq;
    using ImplicitCoordination.PDDLParser.Models;
    
    public class ProblemParser : BaseParser
    {
        public ProblemModel ParseProblem(string filePath)
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
                    ParseInitialState(line, problemModel);
                }
                else if (line.Contains(":goal"))
                {
                    ParseGoalState(line, problemModel);
                }
            }

            return problemModel;
        }

        private void ParseObjects(string line, ProblemModel problemModel)
        {
            var objectData = ExtractData(line);
            problemModel.Objects.AddRange(objectData.Select(obj => new Object(obj, null))); // Adjust as needed for type handling
        }

        private void ParseInitialState(string line, ProblemModel problemModel)
        {
            var initData = ExtractData(line);
            foreach (var pred in initData)
            {
                var parts = pred.Split(' ');
                problemModel.InitialState.Add(new Predicate(parts[0], parts.Skip(1).ToList()));
            }
        }

        private void ParseGoalState(string line, ProblemModel problemModel)
        {
            var goalData = ExtractData(line);
            foreach (var pred in goalData)
            {
                var parts = pred.Split(' ');
                problemModel.GoalState.Add(new Predicate(parts[0], parts.Skip(1).ToList()));
            }
        }

        public override void Parse(string filePath)
        {
            ParseProblem(filePath);
        }
    }
}
