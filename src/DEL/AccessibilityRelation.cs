using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// A dictionary mapping each agent to the set of edges in the accessibility graph.
    /// graph[a] -> (w, v) | w, v \in W.
    /// </summary>
    /// <remarks>Symmetric and transitive edges are omitted to save space.
    /// However, these edges are considered during graph search when evaluating validity of formulas.</remarks>
    public class AccessibilityRelation
    {
        public IDictionary<Agent, HashSet<(IWorld, IWorld)>> graph;

        public AccessibilityRelation(ICollection<Agent> agents, ICollection<IWorld> worlds)
        {
            this.graph = new Dictionary<Agent, HashSet<(IWorld, IWorld)>>();

            foreach (Agent a in agents)
            {
                graph.Add(a, new HashSet<(IWorld, IWorld)>());

                if (worlds != null)
                {
                    foreach (IWorld w in worlds)
                    {
                        // Adding reflexive edges for all agents
                        graph[a].Add((w, w));
                    }
                }
                else { throw new ArgumentNullException(nameof(worlds)); }
            }
        }

        private AccessibilityRelation(ICollection<Agent> agents) : this(agents, new HashSet<IWorld> { }) { }


        public void AddEdge(Agent a, (IWorld, IWorld) edge)
        {
            try
            {
                // Only add (w,v) if edge not already in set.
                // Although this is a set, we must check because to avoid adding symmetric edges, which the set does not consider as equal.
                if (!this.graph[a].ContainsEdge(edge))
                {
                    this.graph[a].Add(edge);
                }
            }
            catch (KeyNotFoundException)
            {
                throw new AgentNotFoundException("Agent does not exist in accessibility graph.");
            }
        }

        public void RemoveEdge(Agent a, (IWorld, IWorld) edge)
        {
            try
            {
                this.graph[a].Remove(edge);
            }
            catch (KeyNotFoundException)
            {
                throw new AgentNotFoundException("Agent does not exist in accessibility graph.");
            }

        }

        public HashSet<(IWorld, IWorld)> GetAccessibilityEdges(Agent a)
        {
            try
            {
                return this.graph[a];
            }
            catch (KeyNotFoundException)
            {
                throw new AgentNotFoundException("Agent does not exist in accessibility graph.");
            }
        }

        /// <summary>
        /// Returns worlds reachable for agent A from world W, as found by BFS on the accessibility graph for agent A
        /// </summary>
        /// <param name="a">The agent for which we want the accessible worlds.</param>
        /// <param name="w">The world from which output worlds are reachable.</param>
        /// <returns>The hashset of worlds that are accessible from w. Implicit transitive edges are taken into account.</returns>
        public HashSet<IWorld> GetAccessibleWorlds(Agent a, IWorld w)
        {
            var edges = this.GetAccessibilityEdges(a);

            var accesibleWorlds = new HashSet<IWorld>();
            var queue = new Queue<IWorld>();

            queue.Enqueue(w);
            IWorld current;

            while (queue.Count != 0)
            {
                current = queue.Dequeue();
                foreach (var (u,v) in edges)
                {
                    if (current == u)
                    {
                        if (!accesibleWorlds.Contains(v))
                        {
                            queue.Enqueue(v);
                            accesibleWorlds.Add(v);
                        }
                    }
                    else if (current == v)
                    {
                        if (!accesibleWorlds.Contains(u))
                        {
                            queue.Enqueue(u);
                            accesibleWorlds.Add(u);
                        }
                    }
                }
            }
            return accesibleWorlds;
        }

        /// <summary>
        /// Yields worlds reachable for agent A from world W, as found by BFS on the accessibility graph for agent A.
        /// Same as method above, but this is a generator.
        /// </summary>
        /// <param name="a">The agent for which we want the accessible worlds.</param>
        /// <param name="w">The world from which output worlds are reachable.</param>
        /// <returns>Iterator with reachable worlds from source world.</returns>
        public IEnumerable<IWorld> GenerateAccessibleWorlds(Agent a, IWorld w)
        {
            var edges = this.GetAccessibilityEdges(a);

            var accesibleWorlds = new HashSet<IWorld>();
            var queue = new Queue<IWorld>();

            queue.Enqueue(w);
            IWorld current;

            while (queue.Count != 0)
            {
                current = queue.Dequeue();
                foreach (var (u, v) in edges)
                {
                    if (current == u)
                    {
                        if (!accesibleWorlds.Contains(v))
                        {
                            queue.Enqueue(v);
                            accesibleWorlds.Add(v);
                            yield return v;
                        }
                    }
                    else if (current == v)
                    {
                        if (!accesibleWorlds.Contains(u))
                        {
                            queue.Enqueue(u);
                            accesibleWorlds.Add(u);
                            yield return v;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an instance of AccessibilityRelation with a dictionary containing the same keys (agents) as in the source graph and empty sets as values (no edges).
        /// </summary>
        /// <returns></returns>
        public AccessibilityRelation CopyEmptyGraph()
        {
            var newGraph = new Dictionary<Agent, HashSet<(IWorld, IWorld)>>();

            foreach (Agent a in this.graph.Keys)
            {
                newGraph[a] = new HashSet<(IWorld, IWorld)>();
            }

            return new AccessibilityRelation(this.graph.Keys);
        }
    }

    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string message)
            : base(message) { }
    }
}
