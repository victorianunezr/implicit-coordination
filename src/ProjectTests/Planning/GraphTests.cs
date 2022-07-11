using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using ImplicitCoordination.Planning;
using ImplicitCoordination.utils;
using NUnit.Framework;
using Action = ImplicitCoordination.DEL.Action;

namespace Planning.Tests
{
    //todo: unit test depth of nodes and root creation
    public class GraphTests
    {
        [SetUp]
        public void ResetCounters()
        {
            Agent.ResetIdCounter();
            Proposition.ResetIdCounter();
            World.ResetIdCounter();
        }

        [Test]
        public void AddOrNode_NodeDoesNotExist_NodeAdded()
        {
            // Arrange
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            AndOrGraph Graph = new AndOrGraph(task);

            State global = task.initialState.GetSetOfGlobals().GetSingleElement();
            AndOrNode s1 = new AndOrNode(global, Graph.root, NodeType.Or);

            // Act - Assert
            Assert.IsTrue(Graph.AddOrNode(s1));
            Assert.AreEqual(1, Graph.root.children.Count);
            Assert.AreEqual(1, Graph.OrNodes.Count);
            Assert.AreEqual(Graph.root, s1.parent);
        }

        //[Test]
        //public void AddOrNode_NodeExists_NodeNotAdded()
        //{
        //    // Arrange
        //    PlanningTask task = PlanningTaskInitializer.DiamondHeist();
        //    Graph Graph = new Graph(task);

        //    State global = task.initialState.GetSetOfGlobals().GetSingleElement();
        //    Node s1 = new Node(global, Graph.root, NodeType.Or);
        //    State global2 = task.initialState.GetSetOfGlobals().GetSingleElement();
        //    Node s2 = new Node(global2, Graph.root, NodeType.Or);
        //    Graph.AddOrNode(s1);

        //    // Act - Assert
        //    Assert.IsFalse(Graph.AddOrNode(s2));
        //    Assert.AreEqual(1, Graph.root.children.Count);
        //    Assert.AreEqual(1, Graph.OrNodes.Count);
        //    Assert.AreEqual(Graph.root, s1.parent);
        //}

        [Test]
        public void AddAndNode_NodeDoesNotExist_NodeAdded()
        {
            // Arrange
            PlanningTask task = PlanningTaskInitializer.DiamondHeist();
            AndOrGraph Graph = new AndOrGraph(task);

            State global = task.initialState.GetSetOfGlobals().GetSingleElement();
            AndOrNode sOR = new AndOrNode(global, Graph.root, NodeType.Or);
            Action cutRed = task.actions.FirstOrDefault(x => x.name.Equals("cutRed"));
            State sPrime = global.ProductUpdate(cutRed);
            AndOrNode sAND = new AndOrNode(sPrime, sOR, NodeType.And, cutRed);

            // Act - Assert
            Assert.IsTrue(Graph.AddAndNode(sAND));
            Assert.AreEqual(2, Graph.AndNodes.Count);
            Assert.AreEqual(sOR, sAND.parent);

        }

        //[Test]
        //public void AddAndNode_NodeEqualsRoot_NodeNotAdded()
        //{
        //    // Arrange - recreate initial state
        //    PlanningTask task = PlanningTaskInitializer.DiamondHeist();
        //    AndOrGraph Graph = new AndOrGraph(task);

        //    // Agents
        //    Agent.ResetIdCounter();
        //    Proposition.ResetIdCounter();
        //    World.ResetIdCounter();

        //    Agent agent1 = new Agent();
        //    Agent agent2 = new Agent();

        //    // Propositions
        //    Proposition r = new Proposition("r");
        //    Proposition l = new Proposition("l");

        //    World w1 = new World();
        //    w1.AddProposition(r);
        //    w1.AddProposition(l);

        //    World w2 = new World();
        //    w2.AddProposition(l);

        //    AccessibilityRelation R = new AccessibilityRelation(new HashSet<Agent>() { agent1, agent2 }, new HashSet<IWorld>() { w1, w2 });
        //    R.AddEdge(agent2, (w1, w2));

        //    State initialState = new State(new HashSet<IWorld>() { w1, w2 }, new HashSet<IWorld>() { w1 }, R);
        //    State global = Graph.root.state.GetSetOfGlobals().GetSingleElement();
        //    AndOrNode s0 = new AndOrNode(initialState, new AndOrNode(global, Graph.root, NodeType.Or), NodeType.And);

        //    // Assert
        //    Assert.IsTrue(Graph.root.state.Equals(s0.state));
        //}

        [Test]
        public void UpdateSolvedDead_Depth1()
        {
            AndOrGraph G = new AndOrGraph(InitTask());

            var globals = G.root.state.GetSetOfGlobals();
            Assert.AreEqual(2, globals.Count());
            var it = globals.GetEnumerator();

            Assert.IsTrue(it.MoveNext());

            State global1 = it.Current;

            Assert.IsTrue(it.MoveNext());

            State global2 = it.Current;

            Assert.IsFalse(global1.Equals(global2));

            AndOrNode newNode1 = new AndOrNode(global1, G.root, NodeType.Or);

            AndOrNode newNode2 = new AndOrNode(global2, G.root, NodeType.Or);

            Assert.IsTrue(G.AddOrNode(newNode1));

            Assert.IsTrue(G.IsSolved(newNode1));

            Assert.IsTrue(G.AddOrNode(newNode2));

            G.UpdateSolvedDead(newNode1);

            // Assert NOT all children of root are solved
            Assert.AreEqual(2, G.root.children.Count);
            Assert.IsFalse(G.root.children.All(x => x.status == NodeStatus.Solved));

            Assert.AreEqual(NodeStatus.Undetermined, G.root.status);

            Assert.IsTrue(G.IsSolved(newNode2));

            G.UpdateSolvedDead(newNode2);

            // Assert all children of root are solved
            Assert.IsTrue(G.root.children.All(x => x.status == NodeStatus.Solved));

            Assert.AreEqual(NodeStatus.Solved, G.root.status);
        }

        private PlanningTask InitTask()
        {
            ResetCounters();

            // Arrange
            Proposition p = new Proposition("p");

            World w = new World(0b010);
            World u = new World(0b11);
            World v = new World(0b110);

            Agent a = new Agent("A");
            Agent b = new Agent("B");

            ICollection<Agent> agents = new HashSet<Agent> { a, b };
            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w, u, v });
            R.AddEdge(a, (w, u));
            R.AddEdge(b, (u, v));

            // Initial State
            State s0 = new State(new HashSet<IWorld> { w, u, v }, new HashSet<IWorld> { w, v }, R);

            Event e = new Event(Formula.Atom(p), new Dictionary<Proposition, bool> { { p, true } });
            HashSet<IWorld> events = new HashSet<IWorld> { e };

            Action act1 = new Action(
                                    events,
                                    events,
                                    new AccessibilityRelation(agents, events),
                                    "act1",
                                    a
                                    );

            Action act2 = new Action(
                         events,
                         events,
                         new AccessibilityRelation(agents, events),
                         "act2",
                         b);

            // Action library
            var actions = new HashSet<Action> { act1, act2 };

            // Goal formula
            Formula gamma = Formula.Not(Formula.Atom(p));

            // Agents
            Dictionary<string, Agent> agentDict = new Dictionary<string, Agent> { { a.name, a }, { b.name, b } };
            PlanningTask task = new PlanningTask(s0, actions, gamma, agentDict);
            return task;

        }
    }
}
