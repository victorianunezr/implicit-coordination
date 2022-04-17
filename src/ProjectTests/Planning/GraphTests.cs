using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using ImplicitCoordination.Planning;
using ImplicitCoordination.utils;
using NUnit.Framework;

namespace Tests.Planning
{
    public class GraphTests
    {

        [Test]
        public void AddOrNode_NodeDoesNotExist_NodeAdded()
        {
            // Arrange
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            Graph Graph = new Graph(task);

            State global = task.initialState.GetSetOfGlobals().GetSingleElement();
            Node s1 = new Node(global, Graph.root, NodeType.Or);

            // Act - Assert
            Assert.IsTrue(Graph.AddOrNode(s1));
            Assert.AreEqual(1, Graph.root.children.Count);
            Assert.AreEqual(1, Graph.OrNodes.Count);
            Assert.AreEqual(Graph.root, s1.parent);
        }

        [Test]
        public void AddOrNode_NodeExists_NodeNotAdded()
        {
            // Arrange
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            Graph Graph = new Graph(task);

            State global = task.initialState.GetSetOfGlobals().GetSingleElement();
            Node s1 = new Node(global, Graph.root, NodeType.Or);
            State global2 = task.initialState.GetSetOfGlobals().GetSingleElement();
            Node s2 = new Node(global2, Graph.root, NodeType.Or);
            Graph.AddOrNode(s1);

            // Act - Assert
            Assert.IsFalse(Graph.AddOrNode(s2));
            Assert.AreEqual(1, Graph.root.children.Count);
            Assert.AreEqual(1, Graph.OrNodes.Count);
            Assert.AreEqual(Graph.root, s1.parent);
        }

        [Test]
        public void AddAndNode_NodeDoesNotExist_NodeAdded()
        {
            // Arrange
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            Graph Graph = new Graph(task);

            State global = task.initialState.GetSetOfGlobals().GetSingleElement();
            Node sOR = new Node(global, Graph.root, NodeType.Or);
            State sPrime = global.ProductUpdate(task.actions["cutRed"]);
            Node sAND = new Node(sPrime, sOR, NodeType.And, task.actions["cutRed"]);

            // Act - Assert
            Assert.IsTrue(Graph.AddAndNode(sAND));
            Assert.AreEqual(2, Graph.AndNodes.Count);
            Assert.AreEqual(sOR, sAND.parent);

        }

        [Test]
        public void AddAndNode_NodeEqualsRoot_NodeNotAdded()
        {
            // Arrange - recreate initial state
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            Graph Graph = new Graph(task);

            // Agents
            Agent.ResetIdCounter();
            Proposition.ResetIdCounter();
            World.ResetIdCounter();

            Agent agent1 = new Agent();
            Agent agent2 = new Agent();

            // Propositions
            Proposition r = new Proposition("r");
            Proposition l = new Proposition("l");

            World w1 = new World();
            w1.AddProposition(r);
            w1.AddProposition(l);

            World w2 = new World();
            w2.AddProposition(l);

            AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent>() { agent1, agent2 }, new HashSet<IWorld>() { w1, w2 });
            R.AddEdge(agent2, (w1, w2));

            State initialState = new State(new HashSet<IWorld>() { w1, w2 }, new HashSet<IWorld>() { w1 }, R);
            State global = Graph.root.state.GetSetOfGlobals().GetSingleElement();
            Node s0 = new Node(initialState, new Node(global, Graph.root, NodeType.Or), NodeType.And);

            // Assert
            Assert.IsFalse(Graph.AddAndNode(s0));
            Assert.AreEqual(0, Graph.root.children.Count);
            Assert.AreEqual(1, Graph.AndNodes.Count);
        }
    }
}
