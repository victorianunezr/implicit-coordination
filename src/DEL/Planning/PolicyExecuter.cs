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
        private static readonly Random randBaseline = new Random();


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
        public static void ExecutePolicy(Graph policy, PlanningTask task)
        {
            Node node = policy.root;
            ICollection<Node> children;

            // Define empty list that will serve as the sequence of actions resulting from the execution
            List<Action> execution = new List<Action>();

            // Collect outgoing edges remaining at a state
            Dictionary<int, WorldEdge> actionsInState = new Dictionary<int, WorldEdge>();

            while (NodeHasOutgoingEdges(node))
            {
                WorldEdge edgeToFollow;
                actionsInState.Clear();

                foreach (World w in node.state.designatedWorlds)
                {
                    foreach (WorldEdge outgoingEdge in w.outgoingEdges)
                    {
                        if (!outgoingEdge.isPruned)
                        {
                            actionsInState.Add(actionsInState.Count, outgoingEdge);
                        }
                    }
                }

                // Select action to execute randomly
                // This is equivalent to all acting agents trying to act asynchronously
                edgeToFollow = actionsInState[random.Next(actionsInState.Count)];
                execution.Add(edgeToFollow.action);
                Console.WriteLine(edgeToFollow.action.name);

                children = node.children;
                node = null;
                // Get the successor state from applying the action
                // we look at the chilren of the current node and select the one that results from the selected action
                foreach (Node child in children)
                {
                    if (child.actionFromParent == edgeToFollow.action)
                    {
                        node = child;
                        break;
                    }
                }
                if (node == null)
                {
                    throw new Exception("no child node was selected");
                }
            }
            // Check that the state reached satisfies the goal formula
            // We only consider the unpruned designated worlds, so we remove pruned worlds
            RemovePrunedDesignatedWorlds(node.state);
            if (task.goalFormula.Evaluate(node.state))
            {
                Console.WriteLine("Successful Execution!");

            }
            else
            {
                 Console.WriteLine("Execution did not reach a goal state.");                               
            }
        }

        // If the formula is true in all remaining designated worlds after pruning
        public static void RemovePrunedDesignatedWorlds(State s)
        {
            foreach (World w in s.designatedWorlds)
            {
                if (w.isPruned)
                {
                    s.designatedWorlds.Remove(w);
                }
            }
        }

        public static void ExecuteBaselinePolicy(AndOrGraph policy, PlanningTask task)
        {
            AndOrNode node = policy.root;

            // Define empty list that will serve as the sequence of actions resulting from the execution
            List<Action> execution = new List<Action>();

            while (node.children.Count > 0)
            {
                
                if (node.type == NodeType.And)
                {
                    // at AND nodes we must visit all children for extracting the policy.
                    // Since we are executing the policy, we follow just one path down the root
                    // for now let's say we pick the first child in the set as the next node to expand
                    // I can just remove the perspective shift to the acting agent such that there is only one children per AND node
                }
                // At OR nodes, we choose child node at random, just as in other policy executer
                else
                {
                    node = node.children.ElementAt(randBaseline.Next(node.children.Count));
                    execution.Add(node.actionFromParent);
                    Console.WriteLine(node.actionFromParent.name);
                }
            }

            // Check that the state reached satisfies the goal formula
            if (task.goalFormula.Evaluate(node.state))
            {
                Console.WriteLine("Successful Execution!");

            }
            else
            {
                Console.WriteLine("Execution did not reach a goal state.");                               
            }

        }

        public static bool NodeHasOutgoingEdges(Node node)
        {
            // Returns true if a node has any unpruned outgoing edgess
            return node.state.possibleWorlds.Cast<World>().Any(w => !w.isPruned && (w.outgoingEdges.Where(e => !e.isPruned).Count() > 0));
        }
    }
}