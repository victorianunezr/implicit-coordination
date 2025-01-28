using System;
using System.Collections.Generic;
using NUnit.Framework;
using ImplicitCoordination.DEL;
using Object = ImplicitCoordination.DEL.Object;

namespace ImplicitCoordination.Tests
{
    [TestFixture]
    public class AtomicFormulaTests
    {
        private World world;
        private State state;

        private Dictionary<GroundPredicate, int> groundPredicateIndex;
        private int nextIndex;

        [SetUp]
        public void Setup()
        {
            groundPredicateIndex = new Dictionary<GroundPredicate, int>();
            nextIndex = 0;
            world = new World("TestWorld", 50);
            state = new State();
            state.possibleWorlds.Add(world);
            state.designatedWorlds.Add(world);
        }

        [Test]
        public void Evaluate_AtomicFormula_WithOneParameter()
        {
            var gp = new GroundPredicate("at", new List<Object> { new Object("pos0", "position") });
            EnableGroundPredicate(world, gp);
            var formula = Formula.Atom(gp);
            bool result = formula.Evaluate(state, world);
            Assert.IsTrue(result);
        }

        [Test]
        public void Evaluate_AtomicFormula_WithMultipleParameters()
        {
            var gp = new GroundPredicate("adjacent", new List<Object>
            {
                new Object("pos0", "position"),
                new Object("pos1", "position")
            });
            EnableGroundPredicate(world, gp);
            var formula = Formula.Atom(gp);
            bool result = formula.Evaluate(state, world);
            Assert.IsTrue(result);
        }

        [Test]
        public void Evaluate_AtomicFormula_WithMissingPredicate()
        {
            var gp = new GroundPredicate("missingPredicate", new List<Object> { new Object("pos0", "position") });
            var formula = Formula.Atom(gp);
            bool result = formula.Evaluate(state, world);
            Assert.IsFalse(result);
        }

        [Test]
        public void Evaluate_AtomicFormula_WithNegatedPredicate()
        {
            var gpPos0 = new GroundPredicate("at", new List<Object> { new Object("pos0", "position") });
            var gpPos1 = new GroundPredicate("at", new List<Object> { new Object("pos1", "position") });
            EnableGroundPredicate(world, gpPos0);
            var formulaPos0 = Formula.Atom(gpPos0);
            var formulaPos1 = Formula.Atom(gpPos1);
            bool result0 = formulaPos0.Evaluate(state, world);
            bool result1 = formulaPos1.Evaluate(state, world);
            Assert.IsTrue(result0);
            Assert.IsFalse(result1);
        }

        private void EnableGroundPredicate(World w, GroundPredicate gp)
        {
            if (!groundPredicateIndex.TryGetValue(gp, out int idx))
            {
                idx = nextIndex++;
                groundPredicateIndex[gp] = idx;
            }
            w.Facts.Set(idx, true);
        }
    }
}
