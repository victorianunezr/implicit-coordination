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
                graph.Add(a, null);
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

        public HashSet<(World, World)> GetAccessibility(Agent a)
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
    }

    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string message)
            : base(message) { }
    }
}
