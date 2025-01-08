using System.Text;
using System.Linq;
using ImplicitCoordination.DEL;
using System.Collections.Generic;

public static class TreeVisualizer
{
    public static string GenerateMermaidDiagram(State rootState)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD");

        var visitedStates = new HashSet<State>();
        var visitedWorlds = new HashSet<IWorld>();

        // Recursive traversal for States and Worlds
        TraverseState(rootState, sb, visitedStates, visitedWorlds);

        // Accessibility Edges
        AppendAccessibilityEdges(rootState, sb);

        sb.AppendLine("```");
        return sb.ToString();
    }

    private static void TraverseState(State state, StringBuilder sb, HashSet<State> visitedStates, HashSet<IWorld> visitedWorlds)
    {
        if (visitedStates.Contains(state)) return;
        visitedStates.Add(state);

        // Add the state
        sb.AppendLine($"subgraph State{state.Id}[\"State {state.Id} - Action: {state.ActionName} - Depth: {state.depth}\"]");

        // Add worlds within the state
        foreach (var world in state.possibleWorlds.OfType<World>())
        {
            if (visitedWorlds.Contains(world)) continue;
            visitedWorlds.Add(world);

            var prunedMark = world.isPruned ? " (Pruned)" : "";
            sb.AppendLine($"\tWorld{world.Id}[\"World {world.Id}<br>Predicates: {{{string.Join(", ", world.predicates)}}}<br>ObjCost: {world.objectiveCost}<br>SubCost: {world.subjectiveCost}{prunedMark}\"]");

            // Add world parent-child relationship
            if (world.incomingEdge != null && world.incomingEdge.parentWorld != null)
            {
                var actionLabel = world.incomingEdge.action != null ? $"Action: {world.incomingEdge.action}" : "Action: N/A";
                sb.AppendLine($"\tWorld{world.incomingEdge.parentWorld.Id} -->|{actionLabel}| World{world.Id}");
            }
        }

        sb.AppendLine("end");

        // Traverse children states
        foreach (var childState in state.Children)
        {
            sb.AppendLine($"State{state.Id} -->|Parent| State{childState.Id}");
            TraverseState(childState, sb, visitedStates, visitedWorlds);
        }
    }

    private static void AppendAccessibilityEdges(State state, StringBuilder sb)
    {
        if (state.accessibility == null) return;

        foreach (var agentPair in state.accessibility.graph)
        {
            var agent = agentPair.Key;
            foreach (var (world1, world2) in agentPair.Value)
            {
                sb.AppendLine($"World{world1.Id} -.->|Agent: {agent.name}| World{world2.Id}");
            }
        }

        // Recursively process child states for accessibility edges
        foreach (var child in state.Children)
        {
            AppendAccessibilityEdges(child, sb);
        }
    }
}
