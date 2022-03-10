﻿using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using NUnit.Framework;
using Action = ImplicitCoordination.DEL.Action;

namespace DEL.Tests
{
    public class ProductUpdateTests
    {
        private Agent a;
        private Agent b;
        private Proposition p;
        private Proposition q;
        private World w;
        private World u;
        private World v;
        private Event e;
        private Event f;
        private State state;
        private Action action;
        private HashSet<IWorld> worlds;
        private HashSet<IWorld> events;

        [OneTimeSetUp]
        public void TestInit()
        {
            this.a = new Agent();
            this.b = new Agent();
            var agents = new HashSet<Agent>() { a, b };

            this.p = new Proposition("p");
            this.q = new Proposition("q");

            this.w = new World(0b11); // p, q
            this.u = new World(0b01); // p, ~q
            this.v = new World(0b10); // ~p, q
            this.worlds = new HashSet<IWorld> { w, u, v };

            this.e = new Event(Formula.Atom(p), new Dictionary<ushort, bool?> { { 0, false } }); // pre: p, post: ~p (q not set)
            this.f = new Event(Formula.Atom(q), new Dictionary<ushort, bool?> { { 0, true }, { 1, false } }); // pre: q, post: p, ~q
            this.events = new HashSet<IWorld> { e, f };

            AccessibilityRelation stateAccessibility = new AccessibilityRelation(agents, worlds);
            stateAccessibility.AddEdge(a, (w, u));
            stateAccessibility.AddEdge(b, (w, v));

            AccessibilityRelation actionAccessibility = new AccessibilityRelation(agents, events);
            actionAccessibility.AddEdge(a, (e, f));
            actionAccessibility.AddEdge(b, (e, f));

            this.state = new State(worlds, new HashSet<IWorld>() { w }, stateAccessibility);
            this.action = new Action(events, new HashSet<IWorld>() { e }, actionAccessibility);
        }

        [Test]
        public void UpdateValuation_EventE()
        {
            // This test does not check that the event is applicable on the world, just that the postcondition is applied correctly
            // Arrange
            World wPrime = this.w.Copy();
            World uPrime = this.u.Copy();
            World vPrime = this.v.Copy();

            // Act
            State.UpdateValuation(wPrime, this.e.post);
            State.UpdateValuation(uPrime, this.e.post);
            State.UpdateValuation(vPrime, this.e.post);

            // Assert
            Assert.IsFalse(wPrime.GetValuation(p));
            Assert.IsTrue(wPrime.GetValuation(q));

            Assert.IsFalse(uPrime.GetValuation(p));
            Assert.IsFalse(uPrime.GetValuation(q));

            Assert.IsFalse(vPrime.GetValuation(p));
            Assert.IsTrue(vPrime.GetValuation(q));
        }

        [Test]
        public void UpdateValuation_EventF()
        {
            // This test does not check that the event is applicable on the world, just that the postcondition is applied correctly
            // Arrange
            World wPrime = this.w.Copy();
            World uPrime = this.u.Copy();
            World vPrime = this.v.Copy();

            // Act
            State.UpdateValuation(wPrime, this.f.post);
            State.UpdateValuation(uPrime, this.f.post);
            State.UpdateValuation(vPrime, this.f.post);

            // Assert
            Assert.IsTrue(wPrime.GetValuation(p));
            Assert.IsFalse(wPrime.GetValuation(q));

            Assert.IsTrue(uPrime.GetValuation(p));
            Assert.IsFalse(uPrime.GetValuation(q));

            Assert.IsTrue(vPrime.GetValuation(p));
            Assert.IsFalse(vPrime.GetValuation(q));
        }

        [Test]
        public void UpdateAccessibility()
        {
            // Arrange
            var newAccessibility = this.state.accessibility.CopyEmptyGraph();
            // Cartesian product in this case includes all ordered pairs (w, e), not only those where precondition holds
            var childWorlds = new HashSet<IWorld>();

            var we = w.CreateChild(e);
            var wf = w.CreateChild(f);
            var ue = u.CreateChild(e);
            var uf = u.CreateChild(f);
            var ve = v.CreateChild(e);
            var vf = v.CreateChild(f);

            childWorlds.Add(we);
            childWorlds.Add(wf);
            childWorlds.Add(ue);
            childWorlds.Add(uf);
            childWorlds.Add(ve);
            childWorlds.Add(vf);

            // Act
            state.UpdateAccessibility(this.action, newAccessibility, childWorlds);

            // Assert
            // Agent a
            // Reflexive edges check
            Assert.IsTrue(newAccessibility.graph[a].Contains((we, we)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((wf, wf)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((ue, ue)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((uf, uf)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((ve, ve)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((vf, vf)));

            // Other edges
            Assert.IsTrue(newAccessibility.graph[a].Contains((we, wf)) || newAccessibility.graph[a].Contains((wf, we)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((we, ue)) || newAccessibility.graph[a].Contains((ue, we)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((we, uf)) || newAccessibility.graph[a].Contains((uf, we)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((wf, ue)) || newAccessibility.graph[a].Contains((ue, wf)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((wf, uf)) || newAccessibility.graph[a].Contains((uf, wf)));
            Assert.IsTrue(newAccessibility.graph[a].Contains((ue, uf)) || newAccessibility.graph[a].Contains((uf, ue)));

            // Should not contain edges to child worlds of v
            Assert.IsFalse(newAccessibility.graph[a].Contains((ve, we)) || newAccessibility.graph[a].Contains((we, ve)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((ve, wf)) || newAccessibility.graph[a].Contains((wf, ve)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((ve, ue)) || newAccessibility.graph[a].Contains((ue, ve)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((ve, uf)) || newAccessibility.graph[a].Contains((uf, ve)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((vf, we)) || newAccessibility.graph[a].Contains((we, vf)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((vf, wf)) || newAccessibility.graph[a].Contains((wf, vf)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((vf, ue)) || newAccessibility.graph[a].Contains((ue, vf)));
            Assert.IsFalse(newAccessibility.graph[a].Contains((vf, uf)) || newAccessibility.graph[a].Contains((uf, vf)));

            // Agent b
            // Reflexive edges check
            Assert.IsTrue(newAccessibility.graph[b].Contains((we, we)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((wf, wf)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((ue, ue)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((uf, uf)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((ve, ve)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((vf, vf)));

            // Other edges
            Assert.IsTrue(newAccessibility.graph[b].Contains((we, wf)) || newAccessibility.graph[b].Contains((wf, we)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((we, ve)) || newAccessibility.graph[b].Contains((ve, we)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((we, vf)) || newAccessibility.graph[b].Contains((vf, we)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((wf, ve)) || newAccessibility.graph[b].Contains((ve, wf)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((wf, vf)) || newAccessibility.graph[b].Contains((vf, wf)));
            Assert.IsTrue(newAccessibility.graph[b].Contains((ve, vf)) || newAccessibility.graph[b].Contains((vf, ve)));

            // Should not contain edges to child worlds of u
            Assert.IsFalse(newAccessibility.graph[b].Contains((ue, we)) || newAccessibility.graph[b].Contains((we, ue)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((ue, wf)) || newAccessibility.graph[b].Contains((wf, ue)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((ue, ve)) || newAccessibility.graph[b].Contains((ve, ue)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((ue, vf)) || newAccessibility.graph[b].Contains((vf, ue)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((uf, we)) || newAccessibility.graph[b].Contains((we, uf)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((uf, wf)) || newAccessibility.graph[b].Contains((wf, uf)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((uf, ve)) || newAccessibility.graph[b].Contains((ve, uf)));
            Assert.IsFalse(newAccessibility.graph[b].Contains((uf, vf)) || newAccessibility.graph[a].Contains((vf, uf)));
        }
    }
}
