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
    public class AndOrGraph
    {
        //private static NodeComparator NodeComparator = new NodeComparator();
        public AndOrNode root;

        /// <summary>
        /// Frontier containing OR AndOrNodes
        /// </summary>
        public Queue<AndOrNode> frontier;

        /// <summary>
        /// AND nodes are the states resulting from a perspective shift followed by a product update
        /// </summary>
        public HashSet<AndOrNode> AndNodes = new HashSet<AndOrNode>();

        /// <summary>
        /// OR nodes are the global states resulting from an AND node
        /// </summary>
        public HashSet<AndOrNode> OrNodes = new HashSet<AndOrNode>();

        /// <summary>
        /// Maintains a set of the leaf nodes in the graph. Dynamically updated during planning, after node expansion.
        /// </summary>
        public ICollection<AndOrNode> SolvedLeafNodes = new HashSet<AndOrNode>();

        /// <summary>
        /// The goal formula
        /// </summary>
        private Formula goalFormula;

        public AndOrGraph(PlanningTask task)
        {
            this.root = AndOrNode.RootNode(task.initialState);
            this.goalFormula = task.goalFormula;
            AddAndNode(this.root);
        }

        /// <summary>
        /// Creates a new AND node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from product update.</param>
        /// <param name="actionToParent">Action that lead to the new node being creted. Used to extract policy.</param>
        /// <param name="parent">Parent node is an OR node. The new node created will be added to the children of the parent.</param>
        private AndOrNode CreateAndNode(State state, Action actionToParent, AndOrNode parent)
        {
            AndOrNode node = new AndOrNode(state, parent, NodeType.And, actionToParent);
            AndNodes.Add(node);
            parent.children.Add(node);
            //AddNode(node);
            return node;
        }

        /// <summary>
        /// Adds a node to the set of AND nodes.
        /// </summary>
        /// <returns>True if the node was added, false otherwise.</returns>>
        public bool AddAndNode(AndOrNode newNode)
        {
            if (newNode.type != NodeType.And)
            {
                throw new IncorrectNodeTypeException();
            }

            // Add if equal node does not exist
            bool add = AndNodes.Add(newNode);
            
            if (add)
            {
                if (!newNode.isRoot)
                {
                    if (newNode.parent == null)
                    {
                        throw new Exception("Parent of non-root AND node cannot be null.");
                    }
                    if (newNode.parent.type != NodeType.Or)
                    {
                        throw new Exception("Parent of non-root AND node must be an OR node.");
                    }
                    newNode.parent.children.Add(newNode);
                }
                //AddNode(newNode);
            }
            return add;
        }

        /// <summary>
        /// Creates a new OR node and updates pointers to parents and children.
        /// </summary>
        /// <param name="state">New state generated from list of globals of parent.</param>
        /// <param name="parent">Parent node is an AND node. The new node created will be added to the children of the parent.</param>
        private AndOrNode CreateOrNode(State state, AndOrNode parent)
        {
            if (state.designatedWorlds.Count != 1)
            {
                throw new Exception("State is not a global state. Cannot create OR node.");
            }

            AndOrNode node = new AndOrNode(state, parent, NodeType.Or, null);
            OrNodes.Add(node);
            parent.children.Add(node);
            //AddNode(node);
            return node;
        }

        /// <summary>
        /// Adds a node to the set of OR nodes.
        /// </summary>
        /// <returns>True if the node was added, false otherwise.</returns>>
        public bool AddOrNode(AndOrNode newNode)
        {
            if (newNode.type != NodeType.Or)
            {
                throw new IncorrectNodeTypeException();
            }
            if (newNode.state.designatedWorlds.Count != 1)
            {
                throw new Exception("State is not a global state. Cannot add to OR nodes.");
            }
            if (newNode.parent == null)
            {
                throw new Exception("Parent of OR node cannot be null.");
            }
            if (newNode.parent.type != NodeType.And)
            {
                throw new Exception("Parent of OR node must be an AND node.");
            }

            // Disable state equality check
            // Add if equal node does not exist
            bool add = OrNodes.Add(newNode);
            if (add)
            {
                //OrNodes.Add(newNode);
                //AddNode(newNode);
                newNode.parent.children.Add(newNode);
            }

            return add;
        }


        //public void AddNode(AndOrNode node)
        //{
        //    if (!nodes.ContainsKey(node.Id))
        //    {
        //        nodes.Add(node.Id, node);
        //    }
        //}

        //public AndOrNode GetNode(ushort nodeId)
        //{
        //    AndOrNode node;
        //    if (nodes.TryGetValue(nodeId, out node))
        //    {
        //        return node;
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException("Node not in graph");
        //    }
        //}

        public void UpdateSolvedDead(AndOrNode node)
        {
            if (node.status == NodeStatus.Undetermined)
            {
                if (IsSolved(node))
                {
                    node.status = NodeStatus.Solved;
                    if (node.parent != null)
                    {
                        UpdateSolvedDead(node.parent);
                    }
                    return;
                }
                else if (IsDead(node))
                {
                    node.status = NodeStatus.Dead;
                    if (node.parent != null)
                    {
                        UpdateSolvedDead(node.parent);
                    }
                    return;
                }
            }
        }

        public bool IsSolved(AndOrNode node)
        {
            if (node.type == NodeType.And)
            {
                foreach (AndOrNode child in node.children)
                {
                    if (child.status != NodeStatus.Solved) return false;
                }
                return true;
            }
            else
            {
                if (goalFormula.Evaluate(node.state)) return true;

                foreach (AndOrNode child in node.children)
                {
                    if (child.status == NodeStatus.Solved) return true;
                }
                return false;
            }
        }

        public bool IsDead(AndOrNode node)
        {
            if (node.type == NodeType.And)
            {
                foreach (AndOrNode child in node.children)
                {
                    if (child.status == NodeStatus.Dead) return true;
                }
                return false;
            }
            else
            {
                foreach (AndOrNode child in node.children)
                {
                    if (child.status != NodeStatus.Dead) return false;
                }
                return !goalFormula.Evaluate(node.state);
            }
        }

        public void UpdateLeafNodes()
        {
            foreach (AndOrNode node in SolvedLeafNodes)
            {
                if (node.children.Count != 0 && node.status == NodeStatus.Solved)
                {
                    SolvedLeafNodes.Remove(node);
                }
            }
            if (SolvedLeafNodes.Count == 0) { throw new Exception("Set of leaf nodes cannot be empty. Something went wrong."); }
        }
    }

    public class IncorrectNodeTypeException : Exception { }
}
