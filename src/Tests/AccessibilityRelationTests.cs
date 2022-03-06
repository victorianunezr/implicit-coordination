using ImplicitCoordination.DEL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ImplicitCoordination.Tests
{
    [TestClass]
    public class AccessibilityRelationTests2
    {
        private Agent a = new Agent();
        private Agent b = new Agent();
        private Agent c = new Agent();
        private World w = new World();
        private World v = new World();

        private AccessibilityRelation accessibility;

        [TestInitialize]
        public void TestInit()
        {
            this.accessibility = new AccessibilityRelation(new HashSet<Agent> { this.a, this.b, this.c });
        }

        [TestMethod]
        public void AddEdge_AgentExists_EdgeAdded()
        {
            // Arrange

            // Act
            this.accessibility.AddEdge(a, (w, v));

            // Assert
            Assert.IsTrue(this.accessibility.graph[a].Contains((w, v)));
        }

        [TestMethod]
        public void AddEdge_MirrorEdgeExists_EdgeNotAdded()
        {
            // Arrange
            this.accessibility.AddEdge(b, (v, w));

            // Act
            this.accessibility.AddEdge(b, (w, v));

            // Assert
            Assert.IsFalse(this.accessibility.graph[b].Contains((w, v)));
        }

        [TestMethod]
        [ExpectedException(typeof(AgentNotFoundException))]
        public void AddEdge_AgentNotInGraph_Throws()
        {
            // Arrange

            // Act
            this.accessibility.AddEdge(new Agent(), (w, v));

            // Assert
        }

        [TestMethod]
        public void RemoveEdge_EdgeExists_EdgeRemoved()
        {
            // Arrange
            this.accessibility.AddEdge(c, (v, w));

            // Act
            this.accessibility.RemoveEdge(c, (v, w));

            // Assert
            Assert.IsFalse(this.accessibility.graph[c].Contains((v, w)));
        }


        [TestMethod]
        [ExpectedException(typeof(AgentNotFoundException))]
        public void RemoveEdge_AgentNotInGraph_Throws()
        {
            // Arrange

            // Act
            this.accessibility.RemoveEdge(new Agent(), (w, v));

            // Assert
        }
    }
}
