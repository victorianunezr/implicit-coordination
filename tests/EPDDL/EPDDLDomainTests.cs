using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace EPDDL.Tests
{
    public class EPDDLDomainTests
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
            var visitor = new MainDefVisitor();

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

            var leftOwner = moveLeft.owners.FirstOrDefault();
            Assert.AreEqual("Alice", leftOwner.name);

            var rightEvent = moveRight.possibleWorlds.First() as Event;
            Assert.AreEqual("e1", rightEvent.name);
            Assert.IsTrue(rightEvent.pre != null); // Check precondition structure
            Assert.IsTrue(rightEvent.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(rightEvent.effect.Count == 2); // Two literals in the effect

            var rightOwner = moveRight.owners.FirstOrDefault();
            Assert.AreEqual("Bob", rightOwner.name);
        }

        [Test]
        public void ParseDomain_ActionWithMultipleOwnersAndEvents_CreatesCorrectActions()
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
                (:action action1
                    :parameters (?pos - position ?next-pos - position)
                    :owners (Alice Charlie)
                    :events (
                        (e1
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                            :effect ((at ?next-pos) (not (at ?pos)))
                        )
                        (e2
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
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
            var visitor = new MainDefVisitor();

            var parsedDomain = visitor.Visit(parser.mainDef()) as Domain;

            Assert.IsNotNull(parsedDomain);
            Assert.AreEqual("lever-domain", parsedDomain.name);

            // Verify actions
            Assert.AreEqual(1, parsedDomain.actions.Count);
            
            var action = parsedDomain.actions.FirstOrDefault(a => a.name == "action1");

            Assert.IsNotNull(action);
            Assert.AreEqual(2, action.possibleWorlds.Count);

            var e1 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e1");
            Assert.AreEqual("e1", e1.name);
            Assert.IsTrue(e1.pre != null);
            Assert.IsTrue(e1.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e1.effect.Count == 2); // Two literals in the effect

            var e2 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e2");
            Assert.AreEqual("e2", e2.name);
            Assert.IsTrue(e2.pre != null);
            Assert.IsTrue(e2.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e2.effect.Count == 2); // Two literals in the effect

            Assert.AreEqual(2, action.owners.Count());
        }

        [Test]
        public void ParseDomain_EmptyEffectAndNoOwners_CreatesCorrectActions()
        {
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
                (:action action1
                    :parameters (?pos - position ?next-pos - position)
                    :owners ()
                    :events (
                        (e1
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                            :effect ()
                        )
                        (e2
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
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
            var visitor = new MainDefVisitor();

            var tree = parser.mainDef();
            // ParserUtils.FormatParseTree(tree, parser);
            var parsedDomain = visitor.Visit(tree) as Domain;

            Assert.IsNotNull(parsedDomain);
            Assert.AreEqual("lever-domain", parsedDomain.name);

            // Verify actions
            Assert.AreEqual(1, parsedDomain.actions.Count);
            
            var action = parsedDomain.actions.FirstOrDefault(a => a.name == "action1");

            Assert.IsNotNull(action);
            Assert.AreEqual(2, action.possibleWorlds.Count);

            var e1 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e1");
            Assert.AreEqual("e1", e1.name);
            Assert.IsTrue(e1.pre != null);
            Assert.IsTrue(e1.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e1.effect.Count == 0); // No literals in the effect

            var e2 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e2");
            Assert.AreEqual("e2", e2.name);
            Assert.IsTrue(e2.pre != null);
            Assert.IsTrue(e2.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e2.effect.Count == 2); // Two literals in the effect

            Assert.AreEqual(0, action.owners.Count());
        }

        [Test]
        public void ParseDomain_WithAccessibilityRelation_CreatesCorrectActions()
        {
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
                (:action action1
                    :parameters (?pos - position ?next-pos - position)
                    :owners (Alice Bob)
                    :events (
                        (e1
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                            :effect ()
                        )
                        (e2
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                            :effect ((at ?next-pos) (not (at ?pos)))
                        )
                        (e3
                            :precondition (and (at ?pos) (not (leftmost ?pos)) (adjacent ?next-pos ?pos))
                            :effect ((at ?next-pos) (not (at ?pos)))
                        )
                    )
                    :accessibility (
                        (e1 e2 Alice)
                        (e2 e3 Bob)
                        (e1 e3 Alice Bob)
                    )
                )
            )";

            // Act: Parse the input
            var inputStream = new AntlrInputStream(leverDomainEPDDL);
            var lexer = new EPDDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new EPDDLParser(tokenStream);
            var visitor = new MainDefVisitor();

            var tree = parser.mainDef();
            // ParserUtils.FormatParseTree(tree, parser);
            var parsedDomain = visitor.Visit(tree) as Domain;

            Assert.IsNotNull(parsedDomain);
            Assert.AreEqual("lever-domain", parsedDomain.name);

            // Verify actions
            Assert.AreEqual(1, parsedDomain.actions.Count);
            
            var action = parsedDomain.actions.FirstOrDefault(a => a.name == "action1");

            Assert.IsNotNull(action);
            Assert.AreEqual(3, action.possibleWorlds.Count);

            var e1 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e1");
            Assert.AreEqual("e1", e1.name);
            Assert.IsTrue(e1.pre != null);
            Assert.IsTrue(e1.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e1.effect.Count == 0); // No literals in the effect

            var e2 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e2");
            Assert.AreEqual("e2", e2.name);
            Assert.IsTrue(e2.pre != null);
            Assert.IsTrue(e2.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e2.effect.Count == 2); // Two literals in the effect

            var e3 = action.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == "e3");
            Assert.AreEqual("e3", e3.name);
            Assert.IsTrue(e3.pre != null);
            Assert.IsTrue(e3.pre.GetFormulaType() == FormulaType.Conjunction);
            Assert.IsTrue(e3.effect.Count == 2); // Two literals in the effect

            Assert.AreEqual(2, action.owners.Count());

            var accessibilityRelation = action.accessibility;

            Assert.IsNotNull(accessibilityRelation, "Accessibility relation should not be null.");

            Agent alice = new Agent("Alice");
            Assert.IsTrue(accessibilityRelation.graph[alice].ContainsEdge((e1, e2)));
            Assert.IsTrue(accessibilityRelation.graph[alice].ContainsEdge((e1, e3)));

            Agent bob = new Agent("Bob");
            Assert.IsTrue(accessibilityRelation.graph[bob].ContainsEdge((e2, e3)));
            Assert.IsTrue(accessibilityRelation.graph[bob].ContainsEdge((e1, e3)));
        }
    }
}