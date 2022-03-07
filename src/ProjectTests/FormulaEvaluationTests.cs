using System.Collections.Generic;
using ImplicitCoordination.DEL;
using NUnit.Framework;

namespace DEL.Tests
{
    [TestFixture]
    public class FormulaEvaluationTests
    {
        private Proposition p;
        private Proposition q;
        private Proposition r;
        private Formula atomP;
        private Formula atomQ;
        private Formula atomR;
        private World w;
        private World u;
        private World v;

        [OneTimeSetUp]
        public void TestInit()
        {
            this.p = new Proposition(name: "p", arity: 0);
            this.q = new Proposition(name: "q", arity: 0);
            this.r = new Proposition(name: "r", arity: 0);
            this.atomP = Formula.Atom(p);
            this.atomQ = Formula.Atom(q);
            this.atomR = Formula.Atom(r);
            this.w = new World(0b111);
            this.u = new World(0b011);
            this.v = new World(0b110);
        }

        [Test]
        public void PropositionalLogic()
        {
            // Evaluating formulas in a world. The setup is:
            // p = false (idx = 0), q = true (idx = 1), r = true (idx = 1)

            Assert.IsFalse(atomP.Evaluate(s: null, w: v));
            Assert.IsTrue(atomQ.Evaluate(s: null, w: v));
            Assert.IsTrue(atomR.Evaluate(s: null, w: v));

            var f1 = Formula.And(atomP, atomQ);
            // P AND Q is false
            Assert.IsFalse(f1.Evaluate(s: null, w: v));

            var f2 = Formula.Or(atomP, atomQ);
            // P OR Q is false
            Assert.IsTrue(f2.Evaluate(s: null, w: v));

            var f3 = Formula.Not(atomP);
            // NOT P is true
            Assert.IsTrue(f3.Evaluate(s: null, w: v));

            var f4 = Formula.And(atomQ, atomR);
            // Q AND R is true
            Assert.IsTrue(f4.Evaluate(s: null, w: v));

            var f5 = Formula.And(Formula.Not(atomP), atomR);
            // (NOT P) AND R is true
            Assert.IsTrue(f5.Evaluate(s: null, w: v));

            var f6 = Formula.Not(Formula.And(atomP, atomQ));
            // NOT (P AND Q) is true
            Assert.IsTrue(f6.Evaluate(s: null, w: v));
        }

        [Test]
        public void EpistemicLogic_FirstOrderKnowledge()
        {
            // Setup state with one agent: State S = {W, R}, W = {w, u, v}, R = {(w, u), (w, v)}
            // Propositions p (LSB),q ,r (MSB). Valuations:
            // w = 111, u = 011, v = 110

            // Arrange
            Agent a = new Agent();
            AccessibilityRelation r = new AccessibilityRelation(new HashSet<Agent>{ a });
            r.AddEdge(a, (w, v));
            r.AddEdge(a, (w, u));
            State s = new State(new HashSet<World> { w, u, v }, null, r);

            var f1 = Formula.Knows(a, atomP);
            var f2 = Formula.Knows(a, atomQ);
            var f3 = Formula.Knows(a, atomR);
            var f4 = Formula.And(Formula.Knows(a, atomP), Formula.Knows(a, atomQ));
            var f5 = Formula.Knows(a, Formula.And(atomP, atomQ));
            var f6 = Formula.Knows(a, Formula.Or(atomP, atomQ));

            // Assert
            Assert.IsFalse(f1.Evaluate(s, w));
            Assert.IsTrue(f1.Evaluate(s, u));
            Assert.IsTrue(f2.Evaluate(s, w));
            Assert.IsFalse(f3.Evaluate(s, w));
            Assert.IsTrue(f3.Evaluate(s, v));
            Assert.IsTrue(f4.Evaluate(s, u));
            Assert.IsFalse(f4.Evaluate(s, w));
            Assert.IsFalse(f4.Evaluate(s, v));
            Assert.IsFalse(f5.Evaluate(s, w));
            Assert.IsTrue(f5.Evaluate(s, u));
            Assert.IsTrue(f6.Evaluate(s, w));
        }

        [Test]
        public void EpistemicLogic_SecondOrderKnowledge()
        {
            // Setup state with agents a and b: State S = {W, R},
            // W = {w, u, v, t},
            // R[a] = {(w, u), (w, v), (t, w)}
            // R[b] = {(u, v), (w, v)}
            // Propositions p (LSB),q ,r (MSB). Valuations:
            // w = 111, u = 011, v = 110, t = 001

            var t = new World(0b100);
            Agent a = new Agent();
            Agent b = new Agent();

            AccessibilityRelation r = new AccessibilityRelation(new HashSet<Agent> { a, b });
            r.AddEdge(a, (w, v));
            r.AddEdge(a, (w, u));
            r.AddEdge(a, (w, t));
            r.AddEdge(b, (w, v));
            r.AddEdge(b, (u, v));
            State s = new State(new HashSet<World> { w, u, v, t }, null, r);

            // Assert
            var f1 = Formula.Knows(a, Formula.Knows(b, atomQ));
            Assert.IsTrue(f1.Evaluate(s, v));
            Assert.IsFalse(f1.Evaluate(s, w));
            Assert.IsTrue(f1.Evaluate(s, u));
            Assert.IsFalse(f1.Evaluate(s, t));

            Assert.IsTrue(Formula.Knows(b, atomR).Evaluate(s, w));
            Assert.IsTrue(Formula.Knows(b, atomR).Evaluate(s, t));
            var f2 = Formula.Knows(a, Formula.Knows(b, atomR));
            Assert.IsTrue(f2.Evaluate(s, t));
            Assert.IsFalse(f2.Evaluate(s, w));
            Assert.IsFalse(f2.Evaluate(s, u));

            var f3 = Formula.Knows(b, Formula.Knows(a, atomP));
            Assert.IsFalse(f3.Evaluate(s, w));
            Assert.IsFalse(f3.Evaluate(s, u));
            Assert.IsFalse(f3.Evaluate(s, t));
            Assert.IsFalse(f3.Evaluate(s, v));
        }
    }
}
