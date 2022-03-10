using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using NUnit.Framework;
using System.Collections.Generic;

namespace DEL.Tests
{
    [TestFixture]
    public class AccessibilityRelationTests
    {
        private Agent a = new Agent();
        private Agent b = new Agent();
        private Agent c = new Agent();
        private World w = new World();
        private World v = new World();
        private World u = new World();


        private AccessibilityRelation accessibility;

        [SetUp]
        public void TestInit()
        {
            this.accessibility = new AccessibilityRelation(new HashSet<Agent> { this.a, this.b, this.c }, new HashSet<IWorld>{ this.w, this.v, this.u });
        }

        [Test]
        public void AddEdge_AgentExists_EdgeAdded()
        {
            // Arrange

            // Act
            this.accessibility.AddEdge(a, (w, v));

            // Assert
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((w, v)));
        }

        [Test]
        public void AddEdge_EdgeAdded_MirrorEdgeExists()
        {
            // Arrange
            this.accessibility.AddEdge(b, (v, w));

            // Act

            // Assert
            Assert.IsTrue(this.accessibility.graph[b].ContainsEdge((w, v)));
        }

        [Test]
        public void AddEdge_AgentNotInGraph_Throws()
        {
            Assert.Throws<AgentNotFoundException>(() =>
            {
                this.accessibility.AddEdge(new Agent(), (w, v));
            });
        }

        [Test]
        public void RemoveEdge_EdgeExists_EdgeRemoved()
        {
            // Arrange
            this.accessibility.AddEdge(c, (v, w));

            // Act
            this.accessibility.RemoveEdge(c, (v, w));

            // Assert
            Assert.IsFalse(this.accessibility.graph[c].ContainsEdge((v, w)));
        }


        [Test]
        public void RemoveEdge_AgentNotInGraph_Throws()
        {

            Assert.Throws<AgentNotFoundException>(() =>
            {
                this.accessibility.RemoveEdge(new Agent(), (w, v));
            });
        }

        [Test]
        public void GetAccessibleWorlds_WorldsReachable_ReturnsNonEmptySet()
        {
            // Arrange
            this.accessibility.AddEdge(a, (v, w));
            this.accessibility.AddEdge(a, (w, v)); // Same edge as before but mirrored.
            this.accessibility.AddEdge(a, (u, w)); // Current edges for a : (w, v), (u, w)

            // Act
            var worlds = this.accessibility.GetAccessibleWorlds(a, w);

            // Assert
            Assert.IsTrue(worlds.Contains(v));
            Assert.IsTrue(worlds.Contains(u));
            Assert.IsTrue(worlds.Contains(w));
            Assert.AreEqual(3, worlds.Count);
        }

        [Test]
        public void GetAccessibleWorlds_NoWorldsReachable_ReturnsEmptySet()
        {
            // Arrange
            this.accessibility.AddEdge(c, (v, u));

            // Act
            var worlds = this.accessibility.GetAccessibleWorlds(c, w);

            // Assert
            Assert.IsFalse(worlds.Contains(v));
            Assert.IsFalse(worlds.Contains(u));

            // Only contains w due to reflexive edge
            Assert.IsTrue(worlds.Contains(w));
            Assert.AreEqual(1, worlds.Count);
        }
    }
}
