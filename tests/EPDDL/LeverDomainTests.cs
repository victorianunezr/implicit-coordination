using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ImplicitCoordination.DEL;
using NUnit.Framework;

namespace EPDDL.Tests
{
    public class LeverDomainTests
    {
        [Test]
        public void ParseLeverDomain_CreatesCorrectDomainStructure()
        {
            // Arrange: Define the EPDDL input for the lever domain
            string leverDomainEPDDL = @"
            (define (domain lever-domain)
                (:requirements :del)
                (:types position)
                (:predicates
                    (at ?pos - position)
                    (adjacent ?pos0 - position ?pos1 - position)
                    (leftmost ?pos - position)
                    (rightmost ?pos - position)
                )
                (:action move-left
                    :parameters (?pos - position ?next-pos - position)
                    :owners (Alice)
                    :events
                        (
                            (e1
                                :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                                :effect ((at ?next-pos) (not (at ?pos)))
                            )
                        )
                    :accessibility ()
                )
                (:action move-right
                    :parameters (?pos - position ?next-pos - position)
                    :owners (Bob)
                    :events
                        (
                            (e1
                                :precondition (and (at ?pos) (not (rightmost ?pos)) (adjacent ?pos ?next-pos))
                                :effect ((at ?next-pos) (not (at ?pos)))
                            )
                        )
                    :accessibility ()
                )
            )";

            // Act: Parse the input
            var inputStream = new AntlrInputStream(leverDomainEPDDL);
            var lexer = new EPDDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new EPDDLParser(tokenStream);
            var visitor = new MainDefVisitor(); // Assuming this is your main visitor class

            var parsedDomain = visitor.Visit(parser.mainDef()) as Domain;

            Assert.IsNotNull(parsedDomain);
            Assert.AreEqual("lever-domain", parsedDomain.name);

            // Verify predicates
            var predicateNames = new HashSet<string> { "at", "adjacent", "leftmost", "rightmost" };
            foreach (var predicate in parsedDomain.Predicates)
            {
                Assert.IsTrue(predicateNames.Contains(predicate.name));
            }

            // Verify actions
            Assert.AreEqual(2, parsedDomain.actions.Count);
            
            var moveLeft = parsedDomain.actions.FirstOrDefault(a => a.name == "move-left");
            var moveRight = parsedDomain.actions.FirstOrDefault(a => a.name == "move-right");

            Assert.IsNotNull(moveLeft);
            Assert.AreEqual(1, moveLeft.possibleWorlds.Count);

            var leftEvent = moveLeft.possibleWorlds.First() as Event;
            Assert.AreEqual("e1", leftEvent.name);
            Assert.IsTrue(leftEvent.pre != null);
            Assert.IsTrue(leftEvent.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(leftEvent.effect.Count == 2); // Two literals in the effect

            Assert.IsNotNull(moveRight);
            Assert.AreEqual(1, moveRight.possibleWorlds.Count);

            var rightEvent = moveRight.possibleWorlds.First() as Event;
            Assert.AreEqual("e1", rightEvent.name);
            Assert.IsTrue(rightEvent.pre != null); // Check precondition structure
            Assert.IsTrue(rightEvent.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(rightEvent.effect.Count == 2); // Two literals in the effect
        }
    }
}