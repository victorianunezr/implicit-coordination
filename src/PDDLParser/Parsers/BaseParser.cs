using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImplicitCoordination.PDDLParser.Parsers
{
    public abstract class BaseParser
    {
        protected string[] ReadFile(string filePath)
        {
            return File.ReadAllLines(filePath);
        }

        protected List<string> ExtractData(string line)
        {
            var match = Regex.Match(line, @"\(([^)]+)\)");
            return match.Success ? match.Groups[1].Value.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
        }

        public abstract void Parse(string filePath);
    }
}
