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
            w.AddProposition(p);
            w.AddProposition(q);
            w.AddProposition(r);

            this.u = new World(0b011);
            u.AddProposition(p);
            u.AddProposition(q);

            this.v = new World(0b110);
            v.AddProposition(q);
            v.AddProposition(r);
        }

        [Test]
        public void PropositionalLogic()
        {
            Proposition.ResetIdCounter();

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
            AccessibilityRelation r = new AccessibilityRelation(new HashSet<Agent>{ a }, new HashSet<IWorld> { w, u, v });
            r.AddEdge(a, (w, v));
            r.AddEdge(a, (w, u));
            State s = new State(new HashSet<IWorld> { w, u, v }, new HashSet<IWorld> { w }, r);

            var f1 = Formula.Knows(a, atomP);
            var f2 = Formula.Knows(a, atomQ);
            var f3 = Formula.Knows(a, atomR);
            var f4 = Formula.And(Formula.Knows(a, atomP), Formula.Knows(a, atomQ));
            var f5 = Formula.Knows(a, Formula.And(atomP, atomQ));
            var f6 = Formula.Knows(a, Formula.Or(atomP, atomQ));

            // Assert
            Assert.IsFalse(f1.Evaluate(s, w));
            Assert.IsFalse(f1.Evaluate(s, u));
            Assert.IsTrue(f2.Evaluate(s, w));
            Assert.IsFalse(f3.Evaluate(s, w));
            Assert.IsFalse(f3.Evaluate(s, v));
            Assert.IsFalse(f4.Evaluate(s, u));
            Assert.IsFalse(f4.Evaluate(s, w));
            Assert.IsFalse(f4.Evaluate(s, v));
            Assert.IsFalse(f5.Evaluate(s, w));
            Assert.IsFalse(f5.Evaluate(s, u));
            Assert.IsTrue(f6.Evaluate(s, w));
        }

        [Test]
        public void EpistemicLogic_SecondOrderKnowledge()
        {
            // Setup state with agents a and b: State S = {W, R},
            // W = {w, u, v, t},
            // R[a] = {(w, u), (w, v)}
            // R[b] = {(u, v), (w, v), (w, t)}
            // Propositions p (LSB),q ,r (MSB). Valuations:
            // w = 111, u = 011, v = 110, t = 001

            var t = new World(0b100);
            t.AddProposition(this.r);
            Agent a = new Agent();
            Agent b = new Agent();

            AccessibilityRelation r = new AccessibilityRelation(new HashSet<Agent> { a, b }, new HashSet<IWorld> { t, u, v, w });
            r.AddEdge(a, (w, v));
            r.AddEdge(a, (w, u));
            r.AddEdge(b, (w, t));
            r.AddEdge(b, (w, v));
            r.AddEdge(b, (u, v));
            State s = new State(new HashSet<IWorld> { w, u, v, t }, null, r);

            // Assert
            var f1 = Formula.Knows(a, Formula.Knows(b, atomQ));
            Assert.IsFalse(f1.Evaluate(s, v));
            Assert.IsFalse(f1.Evaluate(s, w));
            Assert.IsFalse(f1.Evaluate(s, u));
            Assert.IsFalse(f1.Evaluate(s, t));

            Assert.IsFalse(Formula.Knows(b, atomR).Evaluate(s, w));
            Assert.IsFalse(Formula.Knows(b, atomR).Evaluate(s, t));
            var f2 = Formula.Knows(a, Formula.Knows(b, atomR));
            Assert.IsFalse(f2.Evaluate(s, t));
            Assert.IsFalse(f2.Evaluate(s, w));
            Assert.IsFalse(f2.Evaluate(s, u));

            var f3 = Formula.Knows(b, Formula.Knows(a, atomP));
            Assert.IsFalse(f3.Evaluate(s, w));
            Assert.IsFalse(f3.Evaluate(s, u));
            Assert.IsFalse(f3.Evaluate(s, t));
            Assert.IsFalse(f3.Evaluate(s, v));
        }

        [Test]
        public void Transitivity()
        {
            // Setup: agent a, b
            // W = {w, u, v, t}
            // R[a] = {(w, u), (v, t)}
            // R[b] = {(u, v), (v, t)} By transitivity, there is an implicit edge (u, t)
            // Propositions p (LSB),q ,r (MSB). Valuations:
            // w = 111, u = 011, v = 110, t = 100

            // Arrange
            var t = new World(0b100);
            t.AddProposition(this.r);
            Agent a = new Agent();
            Agent b = new Agent();

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, b }, new HashSet<IWorld> { w, u, v, t });
            R.AddEdge(a, (w, u));
            R.AddEdge(a, (v, t));
            R.AddEdge(b, (v, t));
            R.AddEdge(b, (u, v));
            State s = new State(new HashSet<IWorld> { w, u, v, t }, null, R);

            // Assert
            Assert.IsTrue(Formula.Knows(a, atomR).Evaluate(s, t));
            Assert.IsFalse(Formula.Knows(b, atomR).Evaluate(s, t));
            Assert.IsTrue(Formula.Knows(a, atomQ).Evaluate(s, u));
            Assert.IsTrue(Formula.Knows(a, atomP).Evaluate(s, u));
            Assert.IsFalse(Formula.Knows(b, atomQ).Evaluate(s, u));
        }

        [Test]
        public void ValidityInState()
        {
            // Setup: agent a, b
            // W = {w, u, v}
            // R[a] = {(w, u)}
            // R[b] = {(u, v)} 
            // Propositions p (LSB),q ,r (MSB). Valuations:
            // w = 111, u = 011, v = 110

            // Arrange
            Agent a = new Agent();
            Agent b = new Agent();

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, b }, new HashSet<IWorld> { w, u, v });
            R.AddEdge(a, (w, u));
            R.AddEdge(b, (u, v));
            State s = new State(new HashSet<IWorld> { w, u, v }, new HashSet<IWorld> { w, u}, R);

            Assert.IsTrue(atomQ.Evaluate(s));
            Assert.IsTrue(atomP.Evaluate(s));
            Assert.IsFalse(atomR.Evaluate(s));
            Assert.IsTrue(Formula.And(atomP, atomQ).Evaluate(s));
            Assert.IsFalse(Formula.And(atomR, atomQ).Evaluate(s));
        }

        [Test]
        public void DisjunctionOfGoalFormulas()
        {
            // Arrange
            Agent a = new Agent();
            Agent b = new Agent();

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, b }, new HashSet<IWorld> { w, u, v });
            R.AddEdge(a, (w, u));
            R.AddEdge(b, (u, v));


            // Propositions
            Proposition pg1 = new Proposition("g1");
            Proposition pg2 = new Proposition("g2");
            Proposition pg3 = new Proposition("g3");

            w.AddProposition(pg1);
            u.AddProposition(pg2);
            v.AddProposition(pg3);

            State s = new State(new HashSet<IWorld> { w, u, v }, new HashSet<IWorld> { w, u }, R);

            Formula g1 = Formula.Atom(pg1);
            Formula g2 = Formula.Atom(pg2);
            Formula g3 = Formula.Atom(pg3);

            // Goal formulas
            Formula f1 = Formula.And(g1, Formula.Not(atomP)); // not true in w
            Formula f2 = Formula.And(g2, Formula.And(atomP, atomQ)); // true in u
            Formula f3 = Formula.And(g3, atomP); // not true in v

            // Disjunction (goal formula)
            Formula gamma = Formula.Disjunction(new List<Formula> { f1, f2, f3 });

            // Assert
            Assert.IsTrue(gamma.Evaluate(s, u));
            Assert.IsFalse(gamma.Evaluate(s, w));
            Assert.IsFalse(gamma.Evaluate(s, v));
        }

        [Test]
        public void DoesHelperKnowsThatActorKnows()
        {
            // Arrange
            Agent a = new Agent();
            Agent h = new Agent();

            World w1 = new World();
            World w2 = new World();
            Proposition pp = new Proposition("p");

            w1.AddProposition(pp);
            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, h }, new HashSet<IWorld> { w1, w2 });
            R.AddEdge(h, (w1, w2));

            State s = new State(new HashSet<IWorld> { w1, w2 }, new HashSet<IWorld> { w2 }, R);

            Formula pf = Formula.Atom(pp);
            Formula hKnowsP = Formula.Or(Formula.Knows(h, pf), Formula.Knows(h, Formula.Not(pf)));
            Formula aKnowsP = Formula.Or(Formula.Knows(a, pf), Formula.Knows(a, Formula.Not(pf)));

            // Assert
            Assert.IsFalse(hKnowsP.Evaluate(s));
            Assert.IsTrue(aKnowsP.Evaluate(s));
            Assert.IsTrue(Formula.Knows(h, aKnowsP).Evaluate(s));

        }
    }
}
