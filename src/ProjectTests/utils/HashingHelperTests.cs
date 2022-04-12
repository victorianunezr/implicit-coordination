using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace DEL.Utils.Tests
{
    public class HashingHelperTests
    {
        [Test]
        public void SortTuple()
        {
            // Assert
            Assert.AreEqual((1, 2), HashingHelper.SortTuple((2, 1)));
            Assert.AreEqual((1, 1), HashingHelper.SortTuple((1, 1)));
            Assert.AreEqual((1, 2), HashingHelper.SortTuple((1, 2)));
        }

        [Test]
        public void SortTuplesInList()
        {
            // Arrange
            List<(ulong, ulong)> sortedTups = new List<(ulong, ulong)>() { (1, 4), (5, 7), (2, 4), (1, 2), (2, 2) };
            List<(ulong, ulong)> list = new List<(ulong, ulong)>() { (4, 1), (7, 5), (4, 2), (2, 1), (2, 2) };

            // Act
            HashingHelper.SortTuplesInList(list);

            // Assert
            CollectionAssert.AreEqual(sortedTups, list);
        }

        [Test]
        public void SortListOfTuples()
        {
            // Arrange
            List<(ulong, ulong)> sortedList = new List<(ulong, ulong)>() { (1, 2), (1, 4), (2, 2), (2, 4), (2, 5) };
            List<(ulong, ulong)> list = new List<(ulong, ulong)>(){ (4, 1), (2, 5), (4, 2), (2, 1), (2, 2) };

            // Act
            HashingHelper.SortListOfTuples(list);

            // Assert
            CollectionAssert.AreEqual(sortedList, list);
        }

        [Test]
        public void CompressSortedEntry()
        {
            // Arrange
            Agent a = new Agent();
            World w = new World(0);
            World v = new World(4);
            World u = new World(7);
            World t = new World(9);

            HashSet<(World, World)> edges = new HashSet<(World, World)>() { (w,w), (t,t), (v,v), (w,v), (t,v), (u,u), (t,u), (u,w) };
            string expected = "0:(0,0)(0,4)(0,7)(4,4)(4,9)(7,7)(7,9)(9,9)";

            var kvpair = new KeyValuePair<Agent, HashSet<(World,World)>>(a, edges);

            // Act
            string actual = HashingHelper.CompressEntry(kvpair);

            // Assert
            Assert.AreEqual(expected, actual);

        }
    }
}
