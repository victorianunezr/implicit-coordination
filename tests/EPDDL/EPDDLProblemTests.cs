using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace ImplicitCoordination.DEL.Tests
{
    [TestFixture]
    public class ProblemVisitorTests
    {
        [Test]
        public void Test_Lever_Epistemic_Problem_Parsing()
        {
            // Arrange
            string input = @"
            (define (problem lever-epistemic-problem)
                (:domain lever-domain)

                (:requirements :del)

                (:objects
                    pos0 pos1 pos2 pos3 pos4 - position
                )

                (:agents
                    Alice Bob - agent
                )

                (:init
                    :worlds
                    (
                        (w1
                            (
                                (at pos2)
                                (adjacent pos0 pos1)
                                (adjacent pos1 pos2)
                                (adjacent pos2 pos3)
                                (adjacent pos3 pos4)
                                (leftmost pos0)
                                (rightmost pos4)
                                (goal pos0)
                            )
                        )
                        (w2
                            (
                                (at pos2)
                                (adjacent pos0 pos1)
                                (adjacent pos1 pos2)
                                (adjacent pos2 pos3)
                                (adjacent pos3 pos4)
                                (leftmost pos0)
                                (rightmost pos4)
                                (goal pos0)
                                (goal pos4)
                            )
                        )
                        (w3
                            (
                                (at pos2)
                                (adjacent pos0 pos1)
                                (adjacent pos1 pos2)
                                (adjacent pos2 pos3)
                                (adjacent pos3 pos4)
                                (leftmost pos0)
                                (rightmost pos4)
                                (goal pos4)
                            )
                        )
                    )
                    :accessibility
                    (
                        (w1 w2 Alice)
                        (w2 w3 Bob)
                    )
                )

                (:goal
                    (or (and (at pos0) (goal pos0)) (and (at pos4) (goal pos4)))
                )
            )";
            AntlrInputStream inputStream = new AntlrInputStream(input);
            EPDDLLexer lexer = new EPDDLLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            EPDDLParser parser = new EPDDLParser(tokenStream);

            FormulaVisitor formulaVisitor = new FormulaVisitor();
            Domain domain = new Domain { name = "lever-domain" };
            ProblemVisitor problemVisitor = new ProblemVisitor(formulaVisitor, domain);

            // Act
            var context = parser.problemDef();
            var parsedProblem = problemVisitor.VisitProblemDef(context) as Problem;

            // Assert
            Assert.IsNotNull(parsedProblem);
            Assert.AreEqual("lever-epistemic-problem", parsedProblem.name);
            Assert.AreEqual(3, parsedProblem.initialState.possibleWorlds.Count);
            
            // Verify worlds
            var world1 = parsedProblem.initialState.possibleWorlds.FirstOrDefault(w => w.name == "w1") as World;
            Assert.IsNotNull(world1);
            Assert.IsTrue(world1.predicates.Any(p => p.name == "at" && p.Parameters[0].Name == "pos2"));
            Assert.IsTrue(world1.predicates.Any(p => p.name == "goal" && p.Parameters[0].Name == "pos0"));

            var world2 = parsedProblem.initialState.possibleWorlds.FirstOrDefault(w => w.name == "w2") as World;
            Assert.IsNotNull(world2);
            Assert.IsTrue(world2.predicates.Any(p => p.name == "at" && p.Parameters[0].Name == "pos2"));
            Assert.IsTrue(world2.predicates.Any(p => p.name == "goal" && p.Parameters[0].Name == "pos0"));
            Assert.IsTrue(world2.predicates.Any(p => p.name == "goal" && p.Parameters[0].Name == "pos4"));

            var world3 = parsedProblem.initialState.possibleWorlds.FirstOrDefault(w => w.name == "w3") as World;
            Assert.IsNotNull(world3);
            Assert.IsTrue(world3.predicates.Any(p => p.name == "at" && p.Parameters[0].Name == "pos2"));
            Assert.IsTrue(world3.predicates.Any(p => p.name == "goal" && p.Parameters[0].Name == "pos4"));

            var accessibility = parsedProblem.initialState.accessibility;
            Agent alice = new Agent("Alice");
            Agent bob = new Agent("Bob");
            Assert.IsTrue(accessibility.graph[alice].ContainsEdge(world1, world2));
            Assert.IsTrue(accessibility.graph[bob].ContainsEdge(world3, world2));

            // Verify goal formula
            Assert.IsNotNull(parsedProblem.goalFormula);
            Formula expectedGoalFormula = Formula.Or(
                Formula.And(
                    Formula.Atom(new Predicate("at", new List<string> { "pos0" })),
                    Formula.Atom(new Predicate("goal", new List<string> { "pos0" }))
                ),
                Formula.And(
                    Formula.Atom(new Predicate("at", new List<string> { "pos4" })),
                    Formula.Atom(new Predicate("goal", new List<string> { "pos4" }))
                )
            );
            Assert.IsTrue(Formula.AreFormulasEqual(expectedGoalFormula, parsedProblem.goalFormula));
        }
    }
}
