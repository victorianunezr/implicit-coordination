using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace DEL.Utils.Tests
{
    public class HashingHelperTests
    {
        private Agent a;
        private Agent b;
        private Agent c;

        [OneTimeSetUp]
        public void Init()
        {
            Agent.ResetIdCounter();
            World.ResetIdCounter();
            this.a = new Agent();
            this.b = new Agent();
            this.c = new Agent();
        }

        //[Test]
        //public void SortTuple()
        //{
        //    // Assert
        //    Assert.AreEqual((1, 2), HashingHelper.SortTuple((2, 1)));
        //    Assert.AreEqual((1, 1), HashingHelper.SortTuple((1, 1)));
        //    Assert.AreEqual((1, 2), HashingHelper.SortTuple((1, 2)));
        //}

        //[Test]
        //public void SortTuplesInList()
        //{
        //    // Arrange
        //    List<(ulong, ulong)> sortedTups = new List<(ulong, ulong)>() { (1, 4), (5, 7), (2, 4), (1, 2), (2, 2) };
        //    List<(ulong, ulong)> list = new List<(ulong, ulong)>() { (4, 1), (7, 5), (4, 2), (2, 1), (2, 2) };

        //    // Act
        //    HashingHelper.SortTuplesInList(list);

        //    // Assert
        //    CollectionAssert.AreEqual(sortedTups, list);
        //}

        [Test]
        public void SortListOfTuples()
        {
            // Arrange
            List<(ulong, ulong)> sortedList = new List<(ulong, ulong)>() { (1, 2), (1, 4), (2, 2), (2, 4), (2, 5) };
            List<(ulong, ulong)> list = new List<(ulong, ulong)>() { (1, 4), (2, 5), (2, 4), (1, 2), (2, 2) };

            // Act
            HashingHelper.SortListOfTuples(list);

            // Assert
            CollectionAssert.AreEqual(sortedList, list);
        }

        [Test]
        public void KVPairToString()
        {
            // Arrange
            World w = new World(0);
            World v = new World(4);
            World u = new World(7);
            World t = new World(9);

            HashSet<(IWorld, IWorld)> edges = new HashSet<(IWorld, IWorld)>() { (w, w), (t, t), (v, v), (w, v), (t, v), (u, u), (t, u), (u, w) };
            string expected = "0:(0,0)(0,4)(0,7)(4,4)(4,9)(7,7)(7,9)(9,9)";

            var kvpair = new KeyValuePair<Agent, HashSet<(IWorld, IWorld)>>(a, edges);

            // Act
            string actual = HashingHelper.KVPairToString(kvpair);

            // Assert
            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void AccessibilityGraphToString()
        {
            // Arrange
            World w = new World(1);
            World v = new World(3);
            World u = new World(3);
            World t = new World(4);
            World s = new World(5);

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, b, c });

            R.AddEdge(a, (u, w));
            R.AddEdge(a, (w, v));
            R.AddEdge(a, (v, u));
            R.AddEdge(b, (v, u));
            R.AddEdge(b, (t, u));
            R.AddEdge(c, (w, s));
            R.AddEdge(c, (u, s));

            string expected = "0:(1,1)(1,3)(1,3)(3,3)(3,3)(3,3)(4,4)(5,5)1:(1,1)(3,3)(3,3)(3,3)(3,4)(4,4)(5,5)2:(1,1)(1,5)(3,3)(3,3)(3,5)(4,4)(5,5)";

            // Act
            string actual = R.AccessibilityGraphToString();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccessibilityGraphToString_EmptyGraph()
        {
            // Arrange
            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { a, b, c });

            string expected = "0:1:2:";

            // Act
            string actual = R.AccessibilityGraphToString();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HashAccessibilityRelation_EqualHashes()
        {
            // Arrange
            World w = new World(0b11); // p, q
            World u = new World(0b01); // p, ~q
            World v = new World(0b10); // ~p, q
            World t = new World(0b00); // ~p, ~q

            World wP = new World(0b11); // p, q
            World uP = new World(0b01); // p, ~q
            World vP = new World(0b10); // ~p, q
            World tP = new World(0b00); // ~p, ~q

            var agents = new HashSet<Agent>() { a, b };
            var worlds = new HashSet<IWorld> { w, u, v, t };

            AccessibilityRelation R = new AccessibilityRelation(agents, worlds);
            R.AddEdge(a, (w, u));
            R.AddEdge(b, (w, v));
            R.AddEdge(b, (u, t));

            AccessibilityRelation Q = new AccessibilityRelation(new HashSet<Agent>() { a, b });
            Q.AddEdge(a, (wP, uP));
            Q.AddEdge(b, (wP, vP));
            Q.AddEdge(b, (uP, tP));

            // Assert
            Assert.AreEqual(HashingHelper.AccessibilityGraphToString(R), HashingHelper.AccessibilityGraphToString(Q));
            Assert.AreEqual(HashingHelper.HashAccessibilityRelation(R), HashingHelper.HashAccessibilityRelation(Q));

        }
    }
}
