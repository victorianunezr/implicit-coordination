using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using ImplicitCoordination.utils;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class BaselinePlanner : IPlanner
    {
        private AndOrGraph Graph;

        /// <summary>
        /// The planning task to solve
        /// </summary>
        private PlanningTask task;

        public BaselinePlanner(PlanningTask task)
        {
            this.task = task;
        }

        public AndOrGraph Plan()
        {
            Init();

            AndOrNode s;
            State sJ;
            AndOrNode sPrime;
            AndOrNode newGlobal;
            List<Action> actionList = task.actions.ToList();
    

            while (Graph.frontier.Count > 0)
            {
                s = Graph.frontier.Dequeue();
                // Graph.SolvedLeafNodes.Add(s);
                // We put actions in a list to shuffle their order and randomize executions

                actionList.Shuffle();
                foreach (Action action in actionList)
                {
                    sJ = s.state.GetAssociatedLocal(action.owner);
                    //sJ = s.state;
                    if (!sJ.IsApplicable(action,this.task)) continue;
                    // Doing product update on global state (not taking associated local of acting agent)
                    // As such, we only have one designated world and thus only one AND node per OR node
                    sPrime = new AndOrNode(s.state.ProductUpdate(action, sJ.designatedWorlds, this.task), s, NodeType.And, action);                   

                    // Continue if action was not applicable or if s' already exists in AndNodes
                    if (sPrime.state == null || !Graph.AddAndNode(sPrime)) continue;

                    // Graph.SolvedLeafNodes.Add(sPrime);

                    foreach (State global in sPrime.state.Globals())
                    {
                        newGlobal = new AndOrNode(global, sPrime, NodeType.Or);
                        if (!Graph.AddOrNode(newGlobal)) continue;

                        if (task.goalFormula.Evaluate(global))
                        {
                            Graph.UpdateSolvedDead(newGlobal);
                            // Solved leaf nodes are OR nodes that satisfy the goal formula
                            Graph.SolvedLeafNodes.Add(newGlobal);
                        }
                        else
                        {
                            Graph.frontier.Enqueue(newGlobal);
                        }
                    }
                }
                Graph.UpdateSolvedDead(s);
                // Graph.UpdateLeafNodes();

                if (Graph.root.status == NodeStatus.Solved)
                {
                    // assign costs
                    this.AssignCosts();
                    return Graph;
                }
                if (Graph.root.status == NodeStatus.Dead)
                {
                    throw new Exception("Root node is dead. Planning failed.");
                }
            }
            throw new Exception("Root node is dead. Planning failed.");
        }

        public void Init()
        {
            this.Graph = new AndOrGraph(task);

            AndOrNode newNode;

            foreach (State global in Graph.root.state.Globals())
            {
                newNode = new AndOrNode(global, Graph.root, NodeType.Or);

                if (!Graph.AddOrNode(newNode)) continue;

                if (task.goalFormula.Evaluate(Graph.root.state))
                {
                    Graph.UpdateSolvedDead(newNode);
                }
                else
                {
                    Graph.frontier.Enqueue(newNode);
                }
            }
        }

        public void ExtractPolicy()
        {
            this.AssignCosts();
        }

        /// <summary>
        /// Performs a bottom-up iterative traversal of the graph to assign costs to nodes, as defined by DEL-implementation paper.
        /// </summary>
        public void AssignCosts()
        {
            HashSet<AndOrNode> nodesToUpdateNow = new HashSet<AndOrNode>();

            foreach (AndOrNode node in this.Graph.SolvedLeafNodes)
            {
                node.cost = 0;
                nodesToUpdateNow.Add(node.parent);
            }

            HashSet<AndOrNode> nodesToUpdateNext = new HashSet<AndOrNode>();

            // This hashset contains the nodes that we assign the cost to in the i'th iteration
            // Initially, it contain the leaf nodes. Then the nodes are iteratively replaced by their parents.
            // If the parent of a node is null (root), the cost is updated and the node is removed from the set.

            ushort i = 1;

            while (nodesToUpdateNow.Any())
            {
                // todo: update nodes to their parents. 
                foreach (AndOrNode node in nodesToUpdateNow)
                {
                    if (node.type == NodeType.Or && !node.cost.HasValue)
                    {
                        node.cost = i;
                        nodesToUpdateNext.Add(node.parent);
                    }

                    if (node.type == NodeType.And)
                    {
                        // If all children have defined costs, assign cost of child with max cost
                        if (node.children.All(x => x.cost.HasValue))
                        {
                            node.cost = (ushort?)(node.children.MaxBy(x => x.cost).cost + 1);

                            if (node.parent != null)
                            {
                                nodesToUpdateNext.Add(node.parent);
                            }
                        }
                    }
                    //todo: only for debugging. remove later
                    // else
                    // {
                    //     throw new Exception($"Node should have cost {i - 1}, but it has cost {node.cost}");
                    // }
                }
                nodesToUpdateNow = new HashSet<AndOrNode>(nodesToUpdateNext);
                nodesToUpdateNext.Clear();

                i++;
            }
        }
    }

    static class ListExtension
  {
        private static Random rng = new Random();  

        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
  }
}
