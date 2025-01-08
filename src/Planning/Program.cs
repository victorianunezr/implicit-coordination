using System;
using System.IO;
using Antlr4.Runtime;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            string domainFilePath = "../epddl/domain/turnbased-lever.txt";
            var inputDomain = File.ReadAllText(domainFilePath);

            var domainStream = new AntlrInputStream(inputDomain);
            var lexer = new EPDDLLexer(domainStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new EPDDLParser(tokenStream);
            var visitor = new EPDDLVisitor(); // Assuming this is your main visitor class

            Domain parsedDomain = visitor.Visit(parser.mainDef()) as Domain;

            FormulaVisitor formulaVisitor = new FormulaVisitor();
            ProblemVisitor problemVisitor = new ProblemVisitor(formulaVisitor, parsedDomain);
            Console.WriteLine("Domain parsed.");

            var parsedProblem = problemVisitor.VisitProblemDef(parser.problemDef()) as Problem;

            Console.WriteLine("Problem parsed.");
        }
    }
}
