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
        private World w = new World(1);
        private World v = new World(3);
        private World u = new World(3);
        private World t = new World(4);
        private World s = new World(5);


        private AccessibilityRelation accessibility;

        [SetUp]
        public void TestInit()
        {
            this.accessibility = new AccessibilityRelation(new HashSet<Agent> { this.a, this.b, this.c }, new HashSet<IWorld>{ this.w, this.v, this.u, this.t, this.s });
        }

        [Test]
        public void AddEdge_AgentExists_EdgeAdded()
        {
            // Arrange
            World x = new World();
            World y = new World();

            // Act
            this.accessibility.AddEdge(a, (w, v));
            this.accessibility.AddEdge(a, (x, y));


            // Assert
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((w, v)));
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((w, w)));
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((v, v)));
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((w, v)));
            // Check that worlds that were not passed into constructor are also added properly, i.e. reflexive edges are added.
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((x, y)));
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((x, x)));
            Assert.IsTrue(this.accessibility.graph[a].ContainsEdge((y, y)));
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
        public void GetAccessibleWorlds_TransitiveEdges_ReturnsNonEmptySet()
        {

            // Arrange
            this.accessibility.AddEdge(a, (w, u));
            this.accessibility.AddEdge(a, (u, v));
            this.accessibility.AddEdge(a, (v, t));

            // Act
            var worlds = this.accessibility.GetAccessibleWorlds(a, w);

            // Assert
            Assert.IsTrue(worlds.Contains(v));
            Assert.IsTrue(worlds.Contains(u));
            Assert.IsTrue(worlds.Contains(w));
            Assert.IsTrue(worlds.Contains(t));
            Assert.IsFalse(worlds.Contains(s));


            Assert.AreEqual(4, worlds.Count);
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


        [Test]
        public void Equals_EmptyRelations()
        {

        }


        [Test]
        public void Equals_EqualRelations()
        {
            // Arrange
            this.accessibility.AddEdge(a, (w, u));
            this.accessibility.AddEdge(a, (u, v));
            this.accessibility.AddEdge(b, (u, v));
            this.accessibility.AddEdge(b, (v, t));
            this.accessibility.AddEdge(c, (s, w));

            World wP = new World(1);
            World vP = new World(3);
            World uP = new World(3);
            World tP = new World(4);
            World sP = new World(5);

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent> { this.a, this.b, this.c });



        }

        [Test]
        public void Equals_NotEqualRelations()
        {
            this.accessibility.AddEdge(a, (w, u));
            this.accessibility.AddEdge(a, (u, v));
            this.accessibility.AddEdge(b, (u, v));
            this.accessibility.AddEdge(b, (v, t));
            this.accessibility.AddEdge(c, (s, w));

        }
    }
}
