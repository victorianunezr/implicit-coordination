namespace ImplicitCoordination.PDDLParser.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using ImplicitCoordination.PDDLParser.Models;

    public class DomainParser : BaseParser
    {
        public DomainModel ParseDomain(string filePath)
        {
            var domainModel = new DomainModel();
            var lines = ReadFile(filePath);

            foreach (var line in lines)
            {
                if (line.Contains(":predicates"))
                {
                    ParsePredicates(line, domainModel);
                }
                else if (line.Contains(":action"))
                {
                    ParseAction(line, domainModel);
                }
                // Add more parsing logic as needed
            }

            return domainModel;
        }

        private void ParsePredicates(string line, DomainModel domainModel)
        {
            var predicateData = ExtractData(line);
            foreach (var pred in predicateData)
            {
                var parts = pred.Split(' ');
                domainModel.Predicates.Add(new Predicate(parts[0], parts.Skip(1).ToList()));
            }
        }

        private void ParseAction(string line, DomainModel domainModel)
        {
            var actionData = ExtractData(line);
            var name = ExtractName(actionData, "action");
            var parameters = ExtractParameters(actionData, "parameters");
            var precondition = ExtractSection(actionData, "precondition");
            var effect = ExtractSection(actionData, "effect");

            domainModel.Actions.Add(new Action(name, parameters, precondition, effect));
        }

        // Implement other parsing logic as needed

        public override void Parse(string filePath)
        {
            ParseDomain(filePath);
        }

        private string ExtractName(List<string> data, string keyword)
        {
            return data.SkipWhile(s => s != keyword).Skip(1).FirstOrDefault();
        }

        private List<string> ExtractParameters(List<string> data, string keyword)
        {
            var paramIndex = data.IndexOf(keyword) + 1;
            return paramIndex < data.Count ? data[paramIndex].Split(' ').ToList() : new List<string>();
        }

        private string ExtractSection(List<string> data, string section)
        {
            var sectionIndex = data.IndexOf(section) + 1;
            return sectionIndex < data.Count ? data[sectionIndex] : string.Empty;
        }
    }
}
