using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    /// <summary>
    /// Class handling graph structure. Generates new nodes while keeping track of parent/children relations and actions used.
    /// </summary>
    public class Graph
    {
        //private static NodeComparator NodeComparator = new NodeComparator();
        public Node root;

        /// <summary>
        /// Frontier
        /// </summary>
        public Queue<Node> frontier;

        /// <summary>
        /// Maintains a set of the leaf nodes in the graph. Dynamically updated during planning, after node expansion.
        /// </summary>
        public ICollection<Node> leafNodes = new HashSet<Node>();

        /// <summary>
        /// The goal formula
        /// </summary>
        private Formula goalFormula;

        public Graph(PlanningTask task)
        {
            //this.root = Node.CreateRootNode(task.initialState);
            this.goalFormula = task.goalFormula;
        }

        public bool IsSolved(Node node)
        {
            return goalFormula.Evaluate(node.state);
        }

        public void UpdateLeafNodes()
        {
            foreach (Node node in leafNodes)
            {
                if (node.children.Count != 0)
                {
                    leafNodes.Remove(node);
                }
            }
            if (leafNodes.Count == 0) { throw new Exception("Set of leaf nodes cannot be empty. Something went wrong."); }
        }

    }

}
