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
                sb.AppendLine($"{indent} │   │    Predicates: {{{string.Join(", ", world.predicates)}}}");

                // Accessibility Edges
                if (world.outgoingEdges.Any())
                {
                    sb.AppendLine($"{indent} │   │    Accessibility Edges:");
                    foreach (var edge in world.outgoingEdges)
                    {
                        sb.AppendLine($"{indent} │   │      - ({edge.action.owner}) -> World {edge.childWorld}");
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
                        sb.AppendLine($"{indent} │   │      - Action {edge.action.name} -> World {edge.childWorld}");
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
    }
}