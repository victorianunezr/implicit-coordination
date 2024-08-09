namespace ImplicitCoordination.PDDLParser.Parsers
{
    using PDDLParser.Models;
    using System.Collections.Generic;

    public class DomainParser : BaseParser
    {
        private Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();

        public DomainModel ParseDomain(string filePath)
        {
            var domainModel = new DomainModel();
            var lines = ReadFile(filePath);

            foreach (var line in lines)
            {
                if (line.Contains(":types"))
                {
                    ParseTypes(line, domainModel);
                }
                else if (line.Contains(":predicates"))
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

        private void ParseTypes(string line, DomainModel domainModel)
        {
            var typeData = ExtractData(line);
            foreach (var typeName in typeData)
            {
                var type = new Type(typeName);
                domainModel.Types.Add(type);
                typeDictionary[typeName] = type; // Store in a dictionary for quick lookup
            }
        }

        private void ParsePredicates(string line, DomainModel domainModel)
        {
            var predicateData = ExtractData(line);
            foreach (var pred in predicateData)
            {
                var parts = pred.Split(' ');
                var predicateName = parts[0];
                var parameters = new List<(Object, Type)>();

                for (int i = 1; i < parts.Length; i += 2)
                {
                    var paramName = parts[i];
                    var typeName = parts[i + 1].Substring(1); // Remove the hyphen before the type name
                    var type = typeDictionary[typeName];
                    var obj = new Object(paramName, type);
                    parameters.Add((obj, type));
                }

                domainModel.Predicates.Add(new Predicate(predicateName, parameters));
            }
        }
        
        private void ParseAction(string line, DomainModel domainModel)
        {
            // Similar parsing logic can be applied for actions
            // You would handle parameters and effects similar to predicates
        }
    }
}