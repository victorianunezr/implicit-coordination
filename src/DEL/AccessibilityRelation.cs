using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.DEL
{
    public class AccessibilityRelation
    {
        public IDictionary<Agent, HashSet<(World, World)>> graph;

        public AccessibilityRelation(HashSet<Agent> agents)
        {
            this.graph = new Dictionary<Agent, HashSet<(World, World)>>();

            foreach (Agent a in agents)
            {
                graph.Add(a, new HashSet<(World, World)>());
            }
        }

        public void AddEdge(Agent a, (World, World) edge)
        {
            try
            {
                // Only add (w,v) if (v,w) not in set
                (World w, World v) = edge;

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

        public void RemoveEdge(Agent a, (World, World) edge)
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

        public HashSet<(World, World)> GetAccessibilityEdges(Agent a)
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
        public HashSet<World> GetAccessibleWorlds(Agent a, World w)
        {
            var edges = this.GetAccessibilityEdges(a);

            var accesibleWorlds = new HashSet<World>();

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
    }

    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string message)
            : base(message) { }
    }
}
