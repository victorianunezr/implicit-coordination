using System;
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
        Problem Problem;

        [Test]
        public void Test_Lever_Epistemic_Problem_Parsing()
        {
            // Arrange
            string input = @"
            (define (problem lever-epistemic-problem)
                (:domain lever-domain)

                (:objects
                    pos0 pos1 pos2 pos3 pos4 - position
                    alice bob - agent
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
            var domain = new Domain { name = "lever-domain", Predicates = new HashSet<Predicate> {
                new Predicate { name = "at", Parameters = new List<Parameter> { new Parameter("?pos", "position") } },
                new Predicate { name = "adjacent", Parameters = new List<Parameter> { new Parameter("?pos1", "position"), new Parameter("?pos2", "position") } },
                new Predicate { name = "leftmost", Parameters = new List<Parameter> { new Parameter("?pos", "position") } },
                new Predicate { name = "rightmost", Parameters = new List<Parameter> { new Parameter("?pos", "position") } },
                new Predicate { name = "goal", Parameters = new List<Parameter> { new Parameter("?pos", "position") } }
            } };

            ProblemVisitor problemVisitor = new ProblemVisitor(formulaVisitor, domain);

            // Act
            var context = parser.problemDef();
            var parsedProblem = problemVisitor.VisitProblemDef(context) as Problem;
            Problem = parsedProblem;

            // Assert
            Assert.IsNotNull(parsedProblem);
            Assert.AreEqual("lever-epistemic-problem", parsedProblem.name);
            
            // Check objects
            Assert.IsNotNull(parsedProblem.Objects);
            Assert.AreEqual(7, parsedProblem.Objects.Count(),
                "We expect 5 positions (pos0..pos4) plus 2 agents (Alice, Bob).");
            
            var expectedNames = new[] { "pos0", "pos1", "pos2", "pos3", "pos4", "alice", "bob" };
            foreach (var name in expectedNames)
            {
                var obj = parsedProblem.Objects.FirstOrDefault(o => o.Name == name);
                Assert.IsNotNull(obj, $"Object '{name}' was not found among parsed objects.");
            }

            // Check ground predicates
            Assert.IsNotNull(Problem.GroundPredicates, "GroundPredicates set should not be null.");
            Assert.IsTrue(Problem.GroundPredicates.Count > 0,
                "Expected some ground predicates to be generated from domain + objects.");

            Assert.IsNotNull(parsedProblem.initialState);
            Assert.AreEqual(3, parsedProblem.initialState.possibleWorlds.Count);                
            // Verify worlds
            var world1 = parsedProblem.initialState.possibleWorlds.First(w => w.Name == "w1") as World;
            Assert.IsNotNull(world1);

            // Check "at pos2"
            Assert.IsTrue(HasGroundPredicate(world1, "at", "pos2"));
            // Check "goal pos0"
            Assert.IsTrue(HasGroundPredicate(world1, "goal", "pos0"));

            var world2 = parsedProblem.initialState.possibleWorlds.FirstOrDefault(w => w.Name == "w2") as World;
            Assert.IsNotNull(world2);
            Assert.IsTrue(HasGroundPredicate(world2, "at", "pos2"));
            Assert.IsTrue(HasGroundPredicate(world2, "goal", "pos0"));
            Assert.IsTrue(HasGroundPredicate(world2, "goal", "pos4"));

            var world3 = parsedProblem.initialState.possibleWorlds.FirstOrDefault(w => w.Name == "w3") as World;
            Assert.IsNotNull(world3);
            Assert.IsTrue(HasGroundPredicate(world3, "at", "pos2"));
            Assert.IsTrue(HasGroundPredicate(world3, "goal", "pos4"));
            
            var accessibility = parsedProblem.initialState.accessibility;
            Agent alice = new Agent("Alice");
            Agent bob = new Agent("Bob");
            Assert.IsTrue(accessibility.graph[alice].ContainsEdge(world1, world2));
            Assert.IsTrue(accessibility.graph[bob].ContainsEdge(world3, world2));

            // Verify goal formula
            Assert.IsNotNull(parsedProblem.goalFormula);
            Formula expectedGoalFormula = Formula.Or(
                // Left disjunct: (and (at pos0) (goal pos0))
                Formula.And(
                    Formula.Atom(new GroundPredicate(
                        "at", 
                        new List<Object> { new Object("pos0", "position") }
                    )),
                    Formula.Atom(new GroundPredicate(
                        "goal", 
                        new List<Object> { new Object("pos0", "position") }
                    ))
                ),
                // Right disjunct: (and (at pos4) (goal pos4))
                Formula.And(
                    Formula.Atom(new GroundPredicate(
                        "at", 
                        new List<Object> { new Object("pos4", "position") }
                    )),
                    Formula.Atom(new GroundPredicate(
                        "goal", 
                        new List<Object> { new Object("pos4", "position") }
                    ))
                )
            );
            Assert.IsTrue(Formula.AreFormulasEqual(expectedGoalFormula, parsedProblem.goalFormula));
        }

        public bool HasGroundPredicate(World world, string predicateName, params string[] argNames)
        {
            // 1) Convert argNames to Problem Objects
            List<Object> objs = new List<Object>();
            foreach (var argName in argNames)
            {
                var obj = Problem.GetObjectByName(argName);
                if (obj == null)
                {
                    throw new Exception($"Unknown object '{argName}' for predicate '{predicateName}'.");
                }
                objs.Add(obj);
            }

            // 2) Build ground predicate
            var gp = new GroundPredicate(predicateName, objs);

            // 3) Check bit array
            return world.IsTrue(gp);
        }
    }
}
