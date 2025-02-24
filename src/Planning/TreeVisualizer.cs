using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.Planning
{
    public static class TreeVisualizer
    {
        public static void PrintTreeToFile(State rootState, string filePath)
        {
            var sb = new StringBuilder();
            PrintState(rootState, sb, "State #0", null, 0);
            File.WriteAllText(filePath, sb.ToString());
        }

        private static void PrintState(State state, StringBuilder sb, string stateId, State parent, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            sb.AppendLine($"{indent}State {stateId} (Parent: {(parent != null ? $"#{parent.Id}" : "#0")})");

            // Print worlds
            sb.AppendLine($"{indent} ├─ Worlds:");
            foreach (var world in state.possibleWorlds.OfType<World>())
            {
                sb.AppendLine($"{indent} │   ├─ World [Objective={world.objectiveCost}, Subjective={world.subjectiveCost}, Pruned?: {(world.isPruned ? "Yes" : "No")}]");
                sb.AppendLine($"{indent} │   │    Facts:");
                string factsString = world.GetFactsAsString();
                if (!string.IsNullOrEmpty(factsString))
                {
                    var factLines = factsString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var fact in factLines)
                    {
                        sb.AppendLine($"{indent} │   │      {fact}");
                    }
                }
                else
                {
                    sb.AppendLine($"{indent} │   │      (none)");
                }

                // Accessibility Edges
                if (world.outgoingEdges.Any())
                {
                    sb.AppendLine($"{indent} │   │    Accessibility Edges:");
                    foreach (var edge in world.outgoingEdges)
                    {
                        sb.AppendLine($"{indent} │   │      - ({edge.actingAgent}) -> World {edge.childWorld}");
                    }
                }
                else
                {
                    sb.AppendLine($"{indent} │   │    Accessibility Edges: none");
                }

                // Incoming Action
                if (world.incomingEdge != null)
                {
                    sb.AppendLine($"{indent} │   │    Incoming Action:");
                    sb.AppendLine($"{indent} │   │      - From World {world.incomingEdge.parentWorld} via Action {world.incomingEdge.action.name}");
                }

                // Outgoing Actions
                if (world.outgoingEdges.Any())
                {
                    sb.AppendLine($"{indent} │   │    Outgoing Actions:");
                    foreach (var edge in world.outgoingEdges)
                    {
                        sb.AppendLine($"{indent} │   │      - Action ({edge.actingAgent}:{edge.action.name}) -> World {edge.childWorld}");
                    }
                }
            }

            // Print child states
            sb.AppendLine($"{indent} └─ Children States: {string.Join(", ", state.Children.Select(c => $"#{c.Id}"))}");
            foreach (var child in state.Children)
            {
                PrintState(child, sb, $"#{child.Id}", state, indentLevel + 1);
            }
        }

        public static void PrintStateAsYaml(State state, string filePath)
        {
            var sb = new StringBuilder();
            PrintStateAsYaml(state, sb, 0);
            File.WriteAllText(filePath, sb.ToString());
        }

        private static void PrintStateAsYaml(State state, StringBuilder sb, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            sb.AppendLine($"{indent}state s{state.Id}:");
            string indent2 = new string(' ', (indentLevel + 1) * 2);
            
            // Worlds section
            sb.AppendLine($"{indent2}worlds:");
            foreach (var world in state.possibleWorlds.OfType<World>())
            {
                string worldIndent = new string(' ', (indentLevel + 2) * 2);
                sb.AppendLine($"{worldIndent}- name: \"{world.Name}\"");
                sb.AppendLine($"{worldIndent}  objective: {world.objectiveCost}");
                sb.AppendLine($"{worldIndent}  subjective: {world.subjectiveCost}");
                sb.AppendLine($"{worldIndent}  pruned: {world.isPruned}");
                // Facts as a YAML list
                sb.AppendLine($"{worldIndent}  facts:");
                var facts = world.GetFactsAsString()
                                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var fact in facts)
                {
                    sb.AppendLine($"{worldIndent}    - \"{fact}\"");
                }
                // Optionally include accessibility or action info here
            }
            
            // Children section
            if (state.Children.Any())
            {
                sb.AppendLine($"{indent2}children:");
                foreach (var child in state.Children)
                {
                    PrintStateAsYaml(child, sb, indentLevel + 2);
                }
            }
        }

        public static void PrintCurrentStateAsYaml(State state)
        {
            var sb = new StringBuilder();
            string indent = "  ";
            sb.AppendLine("state:");
            sb.AppendLine($"{indent}id: {state.Id}");
            sb.AppendLine($"{indent}worlds:");
            foreach (var world in state.possibleWorlds.OfType<World>())
            {
                string worldIndent = indent + indent;
                sb.AppendLine($"{worldIndent}- name: \"{world.Name}\"");
                sb.AppendLine($"{worldIndent}  objective: {world.objectiveCost}");
                sb.AppendLine($"{worldIndent}  subjective: {world.subjectiveCost}");
                sb.AppendLine($"{worldIndent}  pruned: { (world.isPruned ? "Yes" : "No") }");
                sb.AppendLine($"{worldIndent}  facts:");
                var facts = world.GetFactsAsString()
                                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var fact in facts)
                {
                    sb.AppendLine($"{worldIndent}    - \"{fact}\"");
                }
            }
            Console.WriteLine(sb.ToString());
        }

        public static void ExportWorldGraphAsDot(State rootState, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph Worlds {");
            sb.AppendLine("  node [shape=box];");
            
            // Organize worlds by state and output each state's worlds in a subgraph (rank=same)
            foreach (var state in GetAllStates(rootState))
            {
                var stateWorlds = state.possibleWorlds.OfType<World>().ToList();
                if (stateWorlds.Any())
                {
                    sb.AppendLine("  subgraph cluster_" + state.Id + " {");
                    sb.AppendLine("    rank = same;");
                    foreach (var world in stateWorlds)
                    {
                        sb.AppendLine($"    \"{world.Id}\" [label=\"{world.Name}\\nObj: {world.objectiveCost}\\nSubj: {world.subjectiveCost}\"];");
                    }
                    sb.AppendLine("  }");
                }
            }
            
            // Group and output accessibility edges (within states)
            foreach (var state in GetAllStates(rootState))
            {
                if (state.accessibility != null && state.accessibility.graph != null)
                {
                    foreach (var kvp in state.accessibility.graph)
                    {
                        var edges = kvp.Value
                            .Where(tuple => tuple.Item1 != tuple.Item2)
                            .GroupBy(tuple => new { Source = ((World)tuple.Item1).Id, Target = ((World)tuple.Item2).Id });
                        foreach (var group in edges)
                        {
                            var agentNames = string.Join(", ", group.Select(edge => ((Agent)kvp.Key).name).Distinct());
                            sb.AppendLine($"  \"{group.Key.Source}\" -> \"{group.Key.Target}\" [label=\"{agentNames}\", color=blue, style=dashed];");
                        }
                    }
                }
            }
            
            // Group and output action transition edges (across states)
            var allWorlds = GetAllWorlds(rootState);
            foreach (var group in allWorlds.SelectMany(w => w.outgoingEdges
                                        .Where(e => !e.childWorld.isPruned && e.childWorld.Id != w.Id))
                                        .GroupBy(e => new { Source = ((World)e.parentWorld).Id, Target = ((World)e.childWorld).Id }))
            {
                var labels = group.Select(e => $"{e.actingAgent.name}:{e.action.name}").Distinct();
                string label = string.Join(", ", labels);
                sb.AppendLine($"  \"{group.Key.Source}\" -> \"{group.Key.Target}\" [label=\"{label}\", color=red, style=solid];");
            }
            
            sb.AppendLine("}");
            File.WriteAllText(filePath, sb.ToString());
        }

        private static IEnumerable<World> GetAllWorlds(State root)
        {
            var worlds = new List<World>();
            worlds.AddRange(root.possibleWorlds.OfType<World>());
            foreach (var child in root.Children)
            {
                worlds.AddRange(GetAllWorlds(child));
            }
            return worlds.Distinct();
        }

        private static IEnumerable<State> GetAllStates(State root)
        {
            var states = new List<State> { root };
            foreach (var child in root.Children)
            {
                states.AddRange(GetAllStates(child));
            }
            return states;
        }

   }
}