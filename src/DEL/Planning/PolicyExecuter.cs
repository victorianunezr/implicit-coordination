using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public static class PolicyExecuter
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Given the graph that results from the planning algorithm, we extract a policy by following the remaining
        /// paths from root to leaf, and print the action names in sequence.
        /// When more than one action is prescribed for a state, an action is selected randomly.
        /// </summary>
        /// <remarks>
        /// The execution should take all policies in the policy profile into account (one per agent).
        /// Since in the lever example all agents compute the same policy, we assume there is only one policy in the profile.
        /// The executer currently disregards designated worlds, as it doesn't make a difference in the lever example
        /// <remarks>
        public static void ExecutePolicy(Graph policy)
        {
            Node node = policy.root;

            while (NodeHasOutgoingEdges(node))
            {
                Action actionToExecute;
                // Define empty list that will serve as the sequence of actions resulting from the execution
                List<Action> execution = new List<Action>();
                // Collect actions remaining at a state
                Dictionary<int, Action> actionsInState = new Dictionary<int, Action>();

                foreach (World w in node.state.possibleWorlds)
                {
                    foreach (WorldEdge outgoingEdge in w.outgoingEdges)
                    {
                        if (!outgoingEdge.isPruned)
                        {
                            actionsInState.Add(actionsInState.Count, outgoingEdge.action);
                        }
                    }
                }

                // Select action to execute randomly
                // This is equivalent to all acting agents trying to act asynchronously
                actionToExecute = actionsInState[random.Next(actionsInState.Count)];
                execution.Add(actionToExecute);

                // Get the successor state from applying the action
                // Let's try to follow the outgoing edge from the one of the designated world
                // !! problem: if we follow edge to world, we cannot get the whole state from the world.
                // We might then need to add edges at the state/action level and not only wolrd/event.
                node = 
            }

            // Check that the state reached satisfies the goal formula

        }

        public static bool NodeHasOutgoingEdges(Node node)
        {
            return node.state.possibleWorlds.Cast<World>().Any(w => w.outgoingEdges.Count > 0);
        }
    }
}