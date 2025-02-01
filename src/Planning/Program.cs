using System;
using System.IO;
using Antlr4.Runtime;
using ImplicitCoordination.DEL;
using ImplicitCoordination.Planning;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                // Paths to the domain and problem files
                string domainFilePath = "/Users/victorianunezr/repos/implicit-coordination/epddl/domain/turnbased-lever.txt";
                string problemFilePath = "/Users/victorianunezr/repos/implicit-coordination/epddl/problem/goalrecognition-lever.txt";

                // Parse Domain
                Console.WriteLine("Parsing Domain...");
                var domain = ParseDomain(domainFilePath);
                Console.WriteLine($"Domain '{domain.name}' parsed successfully with {domain.actions.Count} actions and {domain.Predicates.Count} predicates.\n");

                // Parse Problem
                Console.WriteLine("Parsing Problem...");
                var problem = ParseProblem(problemFilePath, domain);
                Console.WriteLine($"Problem '{problem.name}' parsed successfully.");
                Console.WriteLine($"Initial state has {problem.initialState.possibleWorlds.Count} worlds.");
                Console.WriteLine($"Goal formula: {problem.goalFormula}\n");

                // Plan
                Planner planner = new Planner(domain, problem);
                planner.Plan();

                // Print tree
                TreeVisualizer.PrintTreeToFile(planner.Root, "/Users/victorianunezr/repos/implicit-coordination/epddl/output/goalrecognition-lever.txt");
                // // Example access to domain and problem objects
                // Console.WriteLine("Sample Actions in Domain:");
                // foreach (var action in domain.actions)
                // {
                //     Console.WriteLine($"- {action.Name}");
                // }

                // Console.WriteLine("\nWorlds in Initial State:");
                // foreach (var world in problem.initialState.possibleWorlds)
                // {
                //     Console.WriteLine($"- {world.name} with predicates: {string.Join(", ", world.predicates)}");
                // }
            }

        }

        static Domain ParseDomain(string filePath)
        {
            var input = File.ReadAllText(filePath);
            var inputStream = new AntlrInputStream(input);
            var lexer = new EPDDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new EPDDLParser(tokenStream);

            var tree = parser.domainDef();
            var visitor = new DomainVisitor();
            return visitor.Visit(tree) as Domain;
        }

        static Problem ParseProblem(string filePath, Domain domain)
        {
            var input = File.ReadAllText(filePath);
            var inputStream = new AntlrInputStream(input);
            var lexer = new EPDDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new EPDDLParser(tokenStream);

            var tree = parser.problemDef();
            var formulaVisitor = new FormulaVisitor();
            var problemVisitor = new ProblemVisitor(formulaVisitor, domain);

            return problemVisitor.Visit(tree) as Problem;
        }
    }
}
