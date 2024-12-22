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
            Predicate.ResetIdCounter();
            World.ResetIdCounter();
        }

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
            Predicate p = new Predicate("p");

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

            Event e = new Event(Formula.Atom(p), new Dictionary<Predicate, bool> { { p, true } });
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
            //PlanningTask task = new PlanningTask(s0, actions, gamma, agentDict);
            //return task;
            return null;
        }
    }
}
