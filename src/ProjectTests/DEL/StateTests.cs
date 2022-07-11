using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;
using Action = ImplicitCoordination.DEL.Action;

namespace DEL.Tests
{
    public class StateTests
    {
        private Agent a;
        private Agent b;
        private Proposition p;
        private Proposition q;
        private World w;
        private World u;
        private World v;
        private World t;

        private Event e;
        private Event f;
        private State state;
        private Action applicableAction;
        private AccessibilityRelation stateAccessibility;

        private HashSet<IWorld> worlds;
        private HashSet<IWorld> events;

        //todo: test new constructor of State and Action
        //todo: test perspective shifts
        [OneTimeSetUp]
        public void TestInit()
        {
            Proposition.ResetIdCounter();
            Agent.ResetIdCounter();
            World.ResetIdCounter();

            // To visualize scenario, see ProductUpdateExampple.png
            this.a = new Agent();
            this.b = new Agent();
            var agents = new HashSet<Agent>() { a, b };

            this.p = new Proposition("p");
            this.q = new Proposition("q");

            this.w = new World(0b11); // p, q
            this.u = new World(0b01); // p, ~q
            this.v = new World(0b10); // ~p, q
            this.t = new World(0b00); // ~p, ~q

            this.worlds = new HashSet<IWorld> { w, u, v, t };

            this.e = new Event(Formula.Atom(p), new Dictionary<Proposition, bool> { { p, false } }); // pre: p, post: ~p (q not set)
            this.f = new Event(Formula.Atom(q), new Dictionary<Proposition, bool> { { p, true }, { q, false } }); // pre: q, post: p, ~q
            this.events = new HashSet<IWorld> { e, f };

            stateAccessibility = new AccessibilityRelation(agents, worlds);
            stateAccessibility.AddEdge(a, (w, u));
            stateAccessibility.AddEdge(b, (w, v));
            stateAccessibility.AddEdge(b, (u, t));


            AccessibilityRelation actionAccessibility = new AccessibilityRelation(agents, events);
            actionAccessibility.AddEdge(a, (e, f));
            actionAccessibility.AddEdge(b, (e, f));

            this.state = new State(worlds, new HashSet<IWorld>() { w }, stateAccessibility);
            this.applicableAction = new Action(events, new HashSet<IWorld>() { e }, actionAccessibility, "act1", a);
        }

        [Test]
        public void GetAssociatedLocal_AgentA()
        {
            // Act
            State sA = this.state.GetAssociatedLocal(this.a);

            // Assert
            Assert.IsTrue(sA.designatedWorlds.Contains(w));
            Assert.IsTrue(sA.designatedWorlds.Contains(u));
            Assert.IsFalse(sA.designatedWorlds.Contains(v));
            Assert.IsFalse(sA.designatedWorlds.Contains(t));
        }

        [Test]
        public void GetAssociatedLocal_AgentB()
        {
            // Act
            State sB = this.state.GetAssociatedLocal(this.b);

            // Assert
            Assert.IsTrue(sB.designatedWorlds.Contains(w));
            Assert.IsTrue(sB.designatedWorlds.Contains(v));
            Assert.IsFalse(sB.designatedWorlds.Contains(u));
            Assert.IsFalse(sB.designatedWorlds.Contains(t));
        }

        [Test]
        public void GetAssociatedLocal_NotGlobal_Throws()
        {
            // Arrange
            State sA = this.state.GetAssociatedLocal(this.a);

            // Assert
            Assert.Throws<Exception>(() => sA.GetAssociatedLocal(this.a), "The given state is not a global state. It contains 2 designated worlds");
        }

        [Test]
        public void GenerateGlobals()
        {
            // Arrange
            // First get a local state from which globals will be generated
            State sA = this.state.GetAssociatedLocal(this.a);

            int numberOfGlobals = 0;

            // Assert
            foreach (State global in sA.Globals())
            {
                numberOfGlobals++;
                Assert.AreEqual(1, global.designatedWorlds.Count);
                Assert.IsTrue((global.designatedWorlds.Contains(w) && !global.designatedWorlds.Contains(u) && !global.designatedWorlds.Contains(v)) ||
                              (global.designatedWorlds.Contains(u) && !global.designatedWorlds.Contains(w) && !global.designatedWorlds.Contains(v)));
            }

            Assert.AreEqual(2, numberOfGlobals);
        }

        [Test]
        public void PerspectiveShift()
        {
            // Here we are testing perspective shifting from the local state sA to sA^B
            // For a visual repr, see PerspectiveShift.png
            // Arrange
            State sA = this.state.GetAssociatedLocal(this.a);

            // Act
            HashSet<State> perspectiveShiftedStates = sA.PerspectiveShift(b);

            // Assert
            Assert.AreEqual(2, perspectiveShiftedStates.Count);

            var iterator = perspectiveShiftedStates.GetEnumerator();
            iterator.MoveNext();
            State current;
            current = iterator.Current;

            Assert.AreEqual(2, current.designatedWorlds.Count);
            Assert.IsTrue(current.designatedWorlds.Contains(w) && current.designatedWorlds.Contains(v));

            iterator.MoveNext();
            current = iterator.Current;

            Assert.AreEqual(2, current.designatedWorlds.Count);
            Assert.IsTrue(current.designatedWorlds.Contains(t) && current.designatedWorlds.Contains(u));

        }

        //[Test]
        //public void Equals_SmallStates()
        //{
        //    // Arrange
        //    State s1 = new State(new HashSet<IWorld> { w }, new HashSet<IWorld> { w }, new HashSet<Agent>() { a, b });
        //    AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent>() { a, b });
        //    R.AddReflexiveEdgeForAllAgents(w);

        //    World wP = new World(0b11);
        //    State s2 = new State(new HashSet<IWorld> { wP }, new HashSet<IWorld> { wP }, R);
        //    State s3 = new State(new HashSet<IWorld> { wP }, new HashSet<IWorld> { wP }, new HashSet<Agent>() { a });
        //    State s4 = new State(new HashSet<IWorld> { wP }, new HashSet<IWorld> { }, new HashSet<Agent>() { a, b });
        //    State s5 = new State(new HashSet<IWorld> { wP, w }, new HashSet<IWorld> { w }, new HashSet<Agent>() { a, b });


        //    // Assert
        //    Assert.AreEqual(HashingHelper.AccessibilityGraphToString(s1.accessibility), HashingHelper.AccessibilityGraphToString(s2.accessibility));
        //    Assert.AreEqual(HashingHelper.HashAccessibilityRelation(s1.accessibility), HashingHelper.HashAccessibilityRelation(s2.accessibility));
        //    Assert.AreEqual(s1.accessibilityHash, s2.accessibilityHash);
        //    Assert.IsTrue(s1.accessibilityHash.SequenceEqual(s2.accessibilityHash));

        //    Assert.IsTrue(s1.Equals(s2));
        //    Assert.IsFalse(s1.Equals(s3));
        //    Assert.IsFalse(s1.Equals(s4));
        //    Assert.IsFalse(s1.Equals(s5));
        //}

        //[Test]
        //public void Equals_CopyState()
        //{
        //    // Arrange
        //    State sCopy = this.state = new State(worlds, new HashSet<IWorld>() { w }, stateAccessibility);

        //    // Assert
        //    Assert.IsTrue(state.Equals(sCopy));
        //}

        //[Test]
        //public void Equals_EqualStates()
        //{
        //    // Arrange
        //    World wP = new World(0b11); // p, q
        //    World uP = new World(0b01); // p, ~q
        //    World vP = new World(0b10); // ~p, q
        //    World tP = new World(0b00); // ~p, ~q
        //    AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent>() { a, b });
        //    R.AddEdge(a, (wP, uP));
        //    R.AddEdge(b, (wP, vP));
        //    R.AddEdge(b, (uP, tP));

        //    State sP = new State(new HashSet<IWorld> { wP, uP, vP, tP }, new HashSet<IWorld>() { wP }, R);

        //    // Assert
        //    Assert.IsTrue(state.Equals(sP));
        //}

        //[Test]
        //public void Equals_ActionWithNoEffect_EqualWorldsAfter()
        //{
        //    // Arrange
        //    Event eP = new Event(Formula.Atom(p), new Dictionary<ushort, bool> { { 0, true } }); // pre: p, post: p (no change)
        //    Event fP = new Event(Formula.Not(Formula.Atom(p)), new Dictionary<ushort, bool> { { 0, false } }); // pre: ~p, post: ~p (no change)

        //    AccessibilityRelation Q = new AccessibilityRelation(new HashSet<Agent>() { a, b });
        //    Q.AddReflexiveEdgeForAllAgents(eP);
        //    Q.AddEdgeForAllAgents((eP, fP));


        //    Action noEffectAction = new Action(new HashSet<IWorld>() { eP, fP }, new HashSet<IWorld>() { eP }, Q, "noEffect", a);
        //    State sP = state.ProductUpdate(noEffectAction);

        //    // Assert
        //    Assert.IsTrue(state.Equals(sP));
        //}

        //[Test]
        //public void Equals_ApplicableAction_NotEqualWorlds()
        //{
        //    // Arrange
        //    State sP = state.ProductUpdate(applicableAction);

        //    // Assert
        //    Assert.IsFalse(state.Equals(sP));
        //}

        [Test]
        public void ApplicableAction()
        {
            // Assert
            Assert.IsTrue(this.state.IsApplicable(applicableAction));
        }

        [Test]
        public void NonApplicableAction()
        {
            // Arrange
            IWorld g = new Event(Formula.Not(Formula.Atom(this.p)));
            var newEvents = new HashSet<IWorld>() { this.e, this.e, g };
            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent>() { a, b }, newEvents);
            R.AddEdge(a, (e, f));
            R.AddEdge(b, (e, f));
            R.AddEdge(b, (f, g));
            Action nonApplicableAction = new Action(newEvents, new HashSet<IWorld>() { g }, R, "notApplicable", a);

            // Assert
            Assert.IsFalse(this.state.IsApplicable(nonApplicableAction));

        }

        [Test]
        public void PerspectiveShift_SymmetricLever()
        {
            Proposition.ResetIdCounter();
            Agent.ResetIdCounter();
            World.ResetIdCounter();

            // Arrange

            // Agents
            Agent agentL = new Agent("agentLeft");
            Agent agentR = new Agent("agentRight");
            HashSet<Agent> agents = new HashSet<Agent>() { agentL, agentR };

            // Propositions
            Proposition at1 = new Proposition("at1");
            Proposition at2 = new Proposition("at2");
            Proposition at3 = new Proposition("at3");
            Proposition at4 = new Proposition("at4");
            Proposition at5 = new Proposition("at5");
            Proposition goalAt1 = new Proposition("goalAt1");
            Proposition goalAt5 = new Proposition("goalAt5");

            World w1 = new World();
            w1.AddProposition(at3);
            w1.AddProposition(goalAt1);

            World w2 = new World();
            w2.AddProposition(at3);
            w2.AddProposition(goalAt1);
            w2.AddProposition(goalAt5);

            World w3 = new World();
            w3.AddProposition(at3);
            w3.AddProposition(goalAt5);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w1, w2, w3 });
            R.AddEdge(agentL, (w1, w2));
            R.AddEdge(agentR, (w2, w3));
            State initialState = new State(new HashSet<IWorld> { w1, w2, w3 }, new HashSet<IWorld> { w2 }, R);

            // Act

            var shifted = initialState.PerspectiveShift(agentL);

        }
    }
}
