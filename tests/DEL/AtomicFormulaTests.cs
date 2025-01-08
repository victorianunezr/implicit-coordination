using System;
using System.Collections.Generic;
using NUnit.Framework;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.Tests
{
    [TestFixture]
    public class AtomicFormulaTests
    {
        private World world;
        private State state;

        [SetUp]
        public void Setup()
        {
            // Initialize a state and world for testing
            world = new World { Name = "TestWorld" };
            state = new State();

            state.possibleWorlds.Add(world);
            state.designatedWorlds.Add(world);
        }

        [Test]
        public void Evaluate_AtomicFormula_WithOneParameter()
        {
            // Arrange
            var predicate = new Predicate("at", new List<Parameter> { new Parameter("pos0") });
            world.predicates.Add(predicate);

            var atomicFormula = Formula.Atom(predicate);

            // Act
            bool result = atomicFormula.Evaluate(state, world);

            // Assert
            Assert.IsTrue(result, "Atomic formula with one parameter should evaluate to true in the given world.");
        }

        [Test]
        public void Evaluate_AtomicFormula_WithMultipleParameters()
        {
            // Arrange
            var predicate = new Predicate(
                "adjacent",
                new List<Parameter>
                {
                    new Parameter("pos0"),
                    new Parameter("pos1")
                }
            );

            world.predicates.Add(predicate);

            var atomicFormula = Formula.Atom(predicate);

            // Act
            bool result = atomicFormula.Evaluate(state, world);

            // Assert
            Assert.IsTrue(result, "Atomic formula with multiple parameters should evaluate to true in the given world.");
        }

        [Test]
        public void Evaluate_AtomicFormula_WithMissingPredicate()
        {
            // Arrange
            var predicate = new Predicate("missingPredicate", new List<Parameter> { new Parameter("pos0") });
            var atomicFormula = Formula.Atom(predicate);

            // Act
            bool result = atomicFormula.Evaluate(state, world);

            // Assert
            Assert.IsFalse(result, "Atomic formula with a predicate not present in the world should evaluate to false.");
        }

        [Test]
        public void Evaluate_AtomicFormula_WithNegatedPredicate()
        {
            // Arrange
            var pos0 = new Predicate("at", new List<Parameter> { new Parameter("pos0") });
            var pos1 = new Predicate("at", new List<Parameter> { new Parameter("pos1") });

            world.predicates.Add(pos0);

            var pos0Formula = Formula.Atom(pos0);
            var pos1Formula = Formula.Atom(pos1);


            // Act
            bool result0 = pos0Formula.Evaluate(state, world);
            bool result1 = pos1Formula.Evaluate(state, world);

            // Assert
            Assert.IsTrue(result0);
            Assert.IsFalse(result1);
        }
    }
}
