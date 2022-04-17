using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// A dictionary mapping each agent to the set pairs of indistinguishable worlds.
    /// graph[a] -> (w, v) | w, v \in W.
    /// </summary>
    /// <remarks>Symmetric and transitive edges are omitted to save space.
    /// However, these edges are considered during graph search when evaluating validity of formulas.</remarks>
    public class AccessibilityRelation
    {
        public IDictionary<Agent, HashSet<(IWorld, IWorld)>> graph;

        public AccessibilityRelation(ICollection<Agent> agents, ICollection<IWorld> worlds = null)
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
            AddReflexiveEdgeForAllAgents(edge.Item1);
            AddReflexiveEdgeForAllAgents(edge.Item2);
        }


        public void AddEdgeForAllAgents((IWorld, IWorld) edge)
        {
            foreach (var key in this.graph.Keys)
            {
                this.AddEdge(key, edge);
            }
        }


        public void AddReflexiveEdgeForAllAgents(IWorld world)
        {
            foreach (var key in this.graph.Keys)
            {
                graph[key].Add((world, world));
            }
        }

        public void AddReflexive(Agent a, IWorld world)
        {
            this.graph[a].Add((world, world));
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
                foreach (var (u, v) in edges)
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
            return new AccessibilityRelation(this.graph.Keys);
        }

        /// <summary>
        /// Returns an instance of AccessibilityRelation with a dictionary containing the same keys (agents) as in the source graph and only reflexive edges as values.
        /// </summary>
        /// <returns></returns>
        public AccessibilityRelation CopyEmptyGraph(HashSet<IWorld> worlds)
        {
            return new AccessibilityRelation(this.graph.Keys, worlds);
        }


        //public bool Equals(AccessibilityRelation other)
        //{
        //    foreach (var entry in this.graph)
        //    {
        //        try
        //        {
        //            var edges = other.graph[entry.Key];
        //            if (!AreEdgesEqual(entry.Value, edges)) return false;
        //        }
        //        catch (KeyNotFoundException)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}


        //todo: this casting workaround is terrible
        public bool AreEdgesEqual(HashSet<(IWorld, IWorld)> set1, HashSet<(IWorld, IWorld)> set2)
        {
            if (set1.Count != set2.Count) return false;

            foreach (var (w, u) in set1)
            {
                if (!set2.Any(x =>
                       x.Item1.IsEqualTo((World)w) && x.Item2.IsEqualTo((World)u) ||
                       x.Item1.IsEqualTo((World)u) && x.Item2.IsEqualTo((World)w))) return false;
            }
            return true;
        }
    }
    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string message)
            : base(message) { }
    }

    //todo: terrible terrible implementation. Not good having to cast IWorlds to Worlds. Temporary workaround
    public class EdgeComparator : IEqualityComparer<(IWorld, IWorld)>
    {
        public bool Equals((IWorld, IWorld) x, (IWorld, IWorld) y)
        {
            World w1 = (World)x.Item1;
            World w2 = (World)x.Item2;
            World u1 = (World)x.Item1;
            World u2 = (World)x.Item2;

            return w1.valuation.data == u1.valuation.data && w2.valuation.data == u2.valuation.data;
        }

        public int GetHashCode([DisallowNull] (IWorld, IWorld) obj)
        {
            throw new NotImplementedException();
        }
    }
}