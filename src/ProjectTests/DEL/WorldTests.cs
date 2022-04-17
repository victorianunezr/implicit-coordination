using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using NUnit.Framework;

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

            // Act - Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        public void AddProposition()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101); // q is originally false

            // Act
            w.AddProposition(q);
            
            // Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsTrue(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        public void SetValuation()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);

            // Act
            w.SetValuation(p, false);

            // Assert
            Assert.IsFalse(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsTrue(w.IsTrue(r));
        }

        public void SetValuation(ushort propId, bool value)
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);

            // Act
            w.SetValuation(2, false);

            // Assert
            Assert.IsTrue(w.IsTrue(p));
            Assert.IsFalse(w.IsTrue(q));
            Assert.IsFalse(w.IsTrue(r));
        }


        public void IsValid()
        {
            // Arrange
            Proposition p = new Proposition("p");
            Proposition q = new Proposition("q");
            Proposition r = new Proposition("r");
            World w = new World(0b101);
            Agent a = new Agent();
            State s = new State(new HashSet<IWorld> { w }, new HashSet<IWorld> { w }, new HashSet<Agent> { a });
            Formula f = Formula.And(Formula.Atom(p), Formula.Atom(r));

            // Act - Assert
            Assert.IsTrue(w.IsValid(s, f));
            Assert.IsFalse(w.IsValid(s, Formula.Knows(a, Formula.Atom(p))));
        }

        public void Copy()
        {
            // Arrange
            World w = new World(0b101);

            // Act
            World wP = w.Copy();

            // Assert
            Assert.AreEqual(w.valuation, wP.valuation);
            Assert.AreNotEqual(w.Id, wP.Id);
        }

        public void CreateChild()
        {
            // Arrange
            World w = new World(0b101);
            Event e = new Event(Formula.Atom(new Proposition("p")));

            // Act
            World wP = w.CreateChild(e);

            // Assert
            Assert.IsTrue(wP.parentEvent == e);
            Assert.IsTrue(wP.parentWorld == w);
            Assert.AreEqual(wP.valuation, w.valuation);
            Assert.AreNotEqual(wP.Id, w.Id);
        }

        public void Equals(World other)
        {
            // Arrange
            World w = new World(0b101);
            World v = new World(0b101);

            // Act -  Assert
            Assert.AreEqual(w.valuation, v.valuation);
            Assert.AreNotEqual(w.Id, v.Id);
        }
    }
}
