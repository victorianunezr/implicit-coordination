using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.DEL
{
    /// <summary>
    /// A dictionary mapping each agent to the set of edges in the accessibility graph.
    /// graph[a] -> (w, v) | w, v \in W.
    /// </summary>
    /// <remarks></remarks>
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
                // Only add (w,v) if (v,w) not in set
                (IWorld w, IWorld v) = edge;

                if (!this.graph[a].Contains((v, w)))
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
        /// Returns worlds reachable for agent A from world W
        /// </summary>
        /// <param name="a">The agent for which we want the accessible worlds.</param>
        /// <param name="w">The world from which output worlds are reachable.</param>
        /// <returns></returns>
        public HashSet<IWorld> GetAccessibleWorlds(Agent a, IWorld w)
        {
            var edges = this.GetAccessibilityEdges(a);

            var accesibleWorlds = new HashSet<IWorld>();

            foreach (var (u,v) in edges)
            {
                if (w == u)
                {
                    accesibleWorlds.Add(v);
                }
                else if (w == v)
                {
                    accesibleWorlds.Add(u);
                }
            }

            return accesibleWorlds;
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
