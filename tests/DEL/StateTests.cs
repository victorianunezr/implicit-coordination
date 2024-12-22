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
        private Predicate p;
        private Predicate q;
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
            this.a = new Agent("a");
            this.b = new Agent("b");
            var agents = new HashSet<Agent>() { a, b };

            this.p = new Predicate("p");
            this.q = new Predicate("q");

            this.w = new World(0b11); // p, q
            w.AddPredicate(p);
            w.AddPredicate(q);

            this.u = new World(0b01); // p, ~q
            u.AddPredicate(p);

            this.v = new World(0b10); // ~p, q
            v.AddPredicate(q);

            this.t = new World(0b00); // ~p, ~q

            this.worlds = new HashSet<IWorld> { w, u, v, t };

            this.e = new Event{ pre = Formula.Atom(p), effect = new Dictionary<Predicate, bool> { { p, false } }}; // pre: p, post: ~p (q not set)
            this.f = new Event{ pre = Formula.Atom(q), effect = new Dictionary<Predicate, bool> { { p, true }, { q, false } }}; // pre: q, post: p, ~q
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
            Predicate at1 = new Predicate("at1");
            Predicate at2 = new Predicate("at2");
            Predicate at3 = new Predicate("at3");
            Predicate at4 = new Predicate("at4");
            Predicate at5 = new Predicate("at5");
            Predicate goalAt1 = new Predicate("goalAt1");
            Predicate goalAt5 = new Predicate("goalAt5");

            World w1 = new World();
            w1.AddPredicate(at3);
            w1.AddPredicate(goalAt1);

            World w2 = new World();
            w2.AddPredicate(at3);
            w2.AddPredicate(goalAt1);
            w2.AddPredicate(goalAt5);

            World w3 = new World();
            w3.AddPredicate(at3);
            w3.AddPredicate(goalAt5);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w1, w2, w3 });
            R.AddEdge(agentL, (w1, w2));
            R.AddEdge(agentR, (w2, w3));
            State initialState = new State(new HashSet<IWorld> { w1, w2, w3 }, new HashSet<IWorld> { w2 }, R);

            // Act

            var shifted = initialState.PerspectiveShift(agentL);

        }
    }
}
