﻿using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using NUnit.Framework;
using Action = ImplicitCoordination.DEL.Action;

namespace DEL.Tests
{
    public class WorldTests
    {
        [SetUp]
        public void Init()
        {
            Proposition.ResetIdCounter();
        }

        [Test]
        public void GetValuation()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            // Act - Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        [Test]
        public void AddProposition()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101); // q is originally false
            w.AddProposition(p);
            w.AddProposition(r);

            // Act
            w.AddProposition(q);
            
            // Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsTrue(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        [Test]
        public void SetValuation()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            // Act
            w.SetValuation(p, false);

            // Assert
            Assert.IsFalse(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        [Test]
        public void SetValuation_WithPropId()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            // Act
            w.SetValuation(r, false);

            // Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsFalse(w.IsTrue(r));
        }

        [Test]
        public void IsValid()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            Agent a = new Agent();
            State s = new State(new HashSet<IWorld> { w }, new HashSet<IWorld> { w }, new HashSet<Agent> { a });
            Formula f = Formula.And(Formula.Atom(p), Formula.Atom(r));

            // Act - Assert
            Assert.IsTrue(w.IsValid(s, f));
        }

        [Test]
        public void Copy()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");

            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            // Act
            World wP = w.Copy();

            // Assert
            Assert.IsTrue(w.IsEqualTo(wP));
            Assert.AreNotEqual(w.Id, wP.Id);
        }

        [Test]
        public void CreateChild()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");

            Agent a = new Agent();
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            Event e = new Event(Formula.Atom(new Proposition("p")));
            Event f = new Event(Formula.Atom(new Proposition("q")));

            Action action = new Action(new HashSet<IWorld> { e, f }, new HashSet<IWorld> { e }, new HashSet<Agent> { a }, "action", a);

            // Act
            World wP = w.CreateChild(action, e);
            World vP = w.CreateChild(action, f);

            // Assert
            Assert.AreEqual(2, w.outgoingEdges.Count);
            Assert.AreEqual(w, wP.incomingEdge.parentWorld);
            Assert.AreEqual(w, vP.incomingEdge.parentWorld);
            Assert.AreEqual(wP, wP.incomingEdge.childWorld);
            Assert.AreEqual(vP, vP.incomingEdge.childWorld);
            Assert.AreEqual(e, wP.incomingEdge.parentEvent);
            Assert.AreEqual(f, vP.incomingEdge.parentEvent);

        }

        [Test]
        public void Equals()
        {
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");

            // Arrange
            World w = new World(0b101);
            w.AddProposition(p);
            w.AddProposition(r);

            World v = new World(0b101);
            v.AddProposition(p);
            v.AddProposition(r);


            // Act -  Assert
            Assert.IsTrue(w.IsEqualTo(v));
            Assert.AreNotEqual(w.Id, v.Id);
        }

        [Test]
        public void HasAnyApplicableEvent()
        {
            // Arrange
        }
    }
}
