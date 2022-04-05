using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
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
        private Action action;
        private HashSet<IWorld> worlds;
        private HashSet<IWorld> events;

        //todo: test new constructor of State and Action
        //todo: test perspective shifts
        [OneTimeSetUp]
        public void TestInit()
        {
            Proposition.ResetIdCounter();

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

            this.e = new Event(Formula.Atom(p), new Dictionary<ushort, bool> { { 0, false } }); // pre: p, post: ~p (q not set)
            this.f = new Event(Formula.Atom(q), new Dictionary<ushort, bool> { { 0, true }, { 1, false } }); // pre: q, post: p, ~q
            this.events = new HashSet<IWorld> { e, f };

            AccessibilityRelation stateAccessibility = new AccessibilityRelation(agents, worlds);
            stateAccessibility.AddEdge(a, (w, u));
            stateAccessibility.AddEdge(b, (w, v));
            stateAccessibility.AddEdge(b, (u, t));


            AccessibilityRelation actionAccessibility = new AccessibilityRelation(agents, events);
            actionAccessibility.AddEdge(a, (e, f));
            actionAccessibility.AddEdge(b, (e, f));

            this.state = new State(worlds, new HashSet<IWorld>() { w }, stateAccessibility);
            this.action = new Action(events, new HashSet<IWorld>() { e }, actionAccessibility);
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
    }
}
