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
            Proposition currentPos;
            // Define empty list that will serve as the sequence of actions resulting from the execution
            List<Action> execution = new List<Action>();
            HashSet<AndOrNode> childAndNodes = new HashSet<AndOrNode>();
            IEnumerable<AndOrNode> solvedAndNodes = policy.AndNodes.Where(n => n.status == NodeStatus.Solved);
            int currentPosition = task.startingLeverPosition;
            List<AndOrNode> solvedAndNodesList = policy.AndNodes.ToList();
 
            while (node.children.Count > 0)
            {                
                // at AND nodes we must visit all children for extracting the policy.
                // Since we are executing the policy, we follow just one path down the root
                // For all children of the AND node, we select the AND nodes that are solved in the next depth
                // therefore we only visit solved nodes
                foreach (AndOrNode orNode in node.children)
                {
                    foreach (AndOrNode andNode in orNode.children)
                    {
                        childAndNodes.Add(andNode);
                    }
                }
                if (childAndNodes.Count == 0)
                {
                    break;
                }
                // Now we choose a solved and node at random and save the action executed to reach that node
                // This corresponds to agents trying to act asynchronously
                node = childAndNodes.ElementAt(randBaseline.Next(childAndNodes.Count));
                execution.Add(node.actionFromParent);
                Console.WriteLine(node.actionFromParent.name);

                if (node.actionFromParent.name.Equals("Alice:AnnounceGoalAtLeft") || node.actionFromParent.name.Equals("Bob:AnnounceGoalAtRight"))
                {
                    break;
                }
                // The next node is randomly selected from the set of solved nodes, which satisfies the satify the same set of formulas
                if (node.actionFromParent.name.Equals("Alice:Left"))
                {
                    currentPosition--;
                }
                else if (node.actionFromParent.name.Equals("Bob:Right"))
                {
                    currentPosition++;
                }

                if (currentPosition == task.numberOfLeverPositions)
                {
                    currentPos = task.propositions.Get("atn");
                }
                else
                {
                  currentPos = task.propositions.Get("at" + currentPosition.ToString());
                }
                solvedAndNodesList.Shuffle();
                node = null;
                childAndNodes.Clear();

                foreach (AndOrNode potentialNextNode in solvedAndNodesList)
                {
                    // we find a state where the current position proposition holds
                    if (Formula.Atom(currentPos).Evaluate(potentialNextNode.state) && NodeHasGrandchildren(potentialNextNode))
                    {
                        node = potentialNextNode;
                        break;
                    }
                }
                
                if (node == null)
                {
                    throw new Exception("Node should not be null");
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

        public static bool NodeHasGrandchildren(AndOrNode node)
        {
            foreach (AndOrNode child in node.children)
            {
                if (child.children.Any()) {return true;}
            }
            return false;
        }
    }
}