//using System;
//using System.Collections.Generic;
//using ImplicitCoordination.DEL;
//using ImplicitCoordination.utils;
//using Action = ImplicitCoordination.DEL.Action;

//namespace ImplicitCoordination.Planning
//{
//    public class Planner
//    {
//        private Graph Graph;

//        /// <summary>
//        /// The planning task to solve
//        /// </summary>
//        private PlanningTask task;

//        public Planner(PlanningTask task)
//        {
//            this.task = task;
//        }

//        //todo: without contraction, we need a way to check equality of states in order to avoid revisiting states already seen
//        public void Plan()
//        {
//            Init();

//            Node s;
//            State sPrime;
//            State sJ;

//            while (Graph.frontier.Count > 0)
//            {
//                s = Graph.DequeueFrontier();

//                foreach (Action action in task.actions)
//                {
//                    sJ = s.state.GetAssociatedLocal(action.owner);
//                    sPrime = sJ.ProductUpdate(action);

//                    // Continue if action was not applicable or if s' already exists in AndNodes
//                    if (sPrime == null || AndNodes.Contains(sPrime)) continue;

//                    Node newNode = Graph.CreateAndNode(sPrime, action, s);

//                    foreach (State global in sPrime.Globals())
//                    {
//                        if (OrNodes.Contains(global)) continue;

//                        Graph.CreateOrNode(global, newNode);

//                        if (task.goalFormula.Evaluate(sPrime))
//                        {
//                            UpdateSolvedDead(global);
//                        }
//                        else
//                        {
//                            frontier.Enqueue(sPrime);
//                        }
//                    }
//                }
//                UpdateSolvedDead(s);

//                if (IsSolved(root))
//                {
//                    // extract policy
//                }
//                if (IsDead(root))
//                {
//                    throw new Exception("Root node is dead. Planning failed.");
//                }
//            }
//            throw new Exception("Root node is dead. Planning failed.");
//        }

//        public void Init()
//        {
//            this.Graph = new Graph(task.initialState);

//            AndNodes.Add(root);
//            root.type = NodeType.And;

//            foreach (State global in root.state.Globals())
//            {
//                if (OrNodes.Contains(global)) continue;

//                OrNodes.Add(global);
//                global.isOrNode = true;

//                global.parent = root;
//                root.children.Add(global);

//                if (gamma.Evaluate(root))
//                {
//                    UpdateSolvedDead(global);
//                }
//                else
//                {
//                    frontier.Enqueue(root);
//                }
//            }
//        }

//        public void UpdateSolvedDead(State state)
//        {
//            var currentStatus = state.nodeStatus;

//            if (state.isAndNode)
//            {

//            }
//            else if (state.isOrNode)
//            {
//                if (IsSolved(state))
//                {
//                    var newStatus = NodeStatus.Solved;

//                    // If node status was changed, propagate up to parent
//                    if (newStatus != state.nodeStatus)
//                    {
//                        state.nodeStatus = newStatus;
//                        UpdateSolvedDead(state.parent);
//                    }
//                }
//            }
//        }

//        public bool IsSolved(State state)
//        {
//            if (state.isAndNode)
//            {
//                foreach (State child in state.children)
//                {
//                    if (!IsSolved(child)) return false;
//                }
//                return true;
//            }
//            else
//            {
//                if (task.goalFormula.Evaluate(state)) return true;

//                foreach (State child in state.children)
//                {
//                    if (IsSolved(child)) return true;
//                }
//                return false;
//            }
//        }

//        public bool IsDead(State state)
//        {
//            if (state.isAndNode)
//            {
//                foreach (State child in state.children)
//                {
//                    if (IsDead(child)) return true;
//                }
//                return false;
//            }
//            else
//            {
//                foreach (State child in state.children)
//                {
//                    if (!IsDead(child)) return false;
//                }
//                return !task.goalFormula.Evaluate(state);
//            }
//        }
//    }
//}
