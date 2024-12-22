using System;
using System.Collections.Generic;
using System.Linq;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public static class LeverTaskInnitializer
    {
        /// <summary>
        /// Initializes a lever task with n positions and a custom start position
        /// </summary>
        /// <param name="n">Number of positions of the lever</param>
        /// <param name="start">Number of the start position of the lever</param>
        /// <param name="baseline">True if the task is for the baseline planner, which requires formulas to be common knowledge</param>
        /// <returns></returns>
        public static PlanningTask LeverTask(int n, int start, bool baseline)
        {
            if (start >= n)
            {
                throw new Exception($"Start position must be between 1 and {n}");
            }

            // Agents
            Agent agentL = new Agent("agentLeft");
            Agent agentR = new Agent("agentRight");
            HashSet<Agent> agents = new HashSet<Agent>() { agentL, agentR };

            // Init Predicates for lever position
            PredicateRepository PredicateRepository = new PredicateRepository();
            for (int i = 1; i<n; i++)
            {
                PredicateRepository.Add(new Predicate("at" + i.ToString()));
            }
            PredicateRepository.Add(new Predicate("atn"));

            // Predicates for goal configuration
            // Goal at the leftmost position
            Predicate l = new Predicate("l");
            PredicateRepository.Add(l);
            // Goal at the rightmost position
            Predicate r = new Predicate("r");
            PredicateRepository.Add(r);
            // Goal at both ends of the lever
            Predicate lr = new Predicate("lr");
            PredicateRepository.Add(lr);

            // Get start and end positions from Predicate repo
            Predicate at1 = PredicateRepository.Get("at1");
            Predicate atn = PredicateRepository.Get("atn");
            Predicate atStart = PredicateRepository.Get("at" + start.ToString());

            // Initial State
            World w1 = new World();
            w1.AddPredicate(atStart);
            w1.AddPredicate(l);

            World w2 = new World();
            w2.AddPredicate(atStart);
            w2.AddPredicate(lr);

            World w3 = new World();
            w3.AddPredicate(atStart);
            w3.AddPredicate(r);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w1, w2, w3 });
            R.AddEdge(agentL, (w1, w2));
            R.AddEdge(agentR, (w2, w3));
            State initialState = new State(new HashSet<IWorld> { w1, w2, w3 }, new HashSet<IWorld> { w2 }, R);

            Func<World, PlanningTask, Formula> precondDelegateRight = (w,t) => PreconditionDelegateMoveRight(w, t);
            Func<World, PlanningTask, IDictionary<Predicate, bool>?> postcondDelegateRight = (w, t) => PostconditionDelegateMoveRight(w, t);

            Func<World, PlanningTask, Formula> precondDelegateLeft = (w, t) => PreconditionDelegateMoveLeft(w, t);
            Func<World, PlanningTask, IDictionary<Predicate, bool>?> postcondDelegateLeft = (w, t) => PostconditionDelegateMoveLeft(w, t);

            // Actions
            Event pullRightEvent = new Event();
            AccessibilityRelation moveRightRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { pullRightEvent });
            Action pullRight = new Action(new HashSet<IWorld> { pullRightEvent }, new HashSet<IWorld> { pullRightEvent }, moveRightRelation, "Bob:Right", agentR);

            Event pullLeftEvent = new Event();
            AccessibilityRelation moveLeftRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { pullLeftEvent });
            Action pullLeft = new Action(new HashSet<IWorld> { pullLeftEvent }, new HashSet<IWorld> { pullLeftEvent }, moveLeftRelation, "Alice:Left", agentL);

            Event skipEventAlice = new Event(Formula.Top(), null);
            AccessibilityRelation skipRAlice = new AccessibilityRelation(agents, new HashSet<IWorld> { skipEventAlice });
            Action skipAlice = new Action(new HashSet<IWorld> { skipEventAlice }, new HashSet<IWorld> { skipEventAlice }, skipRAlice, "Alice:Skip", agentL);

            Event skipEventBob = new Event(Formula.Top(), null);
            AccessibilityRelation skipRBob = new AccessibilityRelation(agents, new HashSet<IWorld> { skipEventAlice });
            Action skipBob = new Action(new HashSet<IWorld> { skipEventAlice }, new HashSet<IWorld> { skipEventAlice }, skipRAlice, "Bob:Skip", agentR);

            HashSet<Action> actions = new HashSet<Action> { pullLeft, pullRight, skipAlice, skipBob };

            // Public announcements that goal has been reached, for baseline planner
            if (baseline)
            {
                // To announce success at leftmost position, lever should be at1
                Event announceLeftEvent = new Event(Formula.And(Formula.Atom(at1),Formula.Knows(agentL, Formula.Or(Formula.Atom(l), Formula.Atom(lr)))), 
                                                    new Dictionary<Predicate, bool> { {l, true}, {r, false}});
                AccessibilityRelation announceLeftRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { announceLeftEvent });
                Action announceLeftGoal = new Action(new HashSet<IWorld> { announceLeftEvent }, new HashSet<IWorld> { announceLeftEvent }, announceLeftRelation, "Alice:AnnounceGoalAtLeft", agentL);

                Event announceRightEvent = new Event(Formula.And(Formula.Atom(atn),Formula.Knows(agentR, Formula.Or(Formula.Atom(r), Formula.Atom(lr)))), 
                                                    new Dictionary<Predicate, bool> { {r, true}, {l, false}});
                AccessibilityRelation announceRightRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { announceRightEvent });
                Action announceRightGoal = new Action(new HashSet<IWorld> { announceRightEvent }, new HashSet<IWorld> { announceRightEvent }, announceRightRelation, "Bob:AnnounceGoalAtRight", agentR);
            
                actions.Add(announceLeftGoal);
                actions.Add(announceRightGoal);

                if (n < 15)
                {
                    initialState.designatedWorlds.Add(w1);
                    initialState.designatedWorlds.Add(w3);
                }
            }

            // Atomic formulas
            Formula fAt1 = Formula.Atom(at1);
            Formula fAtn = Formula.Atom(atn);

            Formula goalAtLeft = Formula.Atom(l);
            Formula goalAtRight = Formula.Atom(r);
            Formula goalLeftAndRight = Formula.Atom(lr);

            Formula gamma;
            if (!baseline)
            {
                // not baseline planner. Gamma is not a common knowledge formula
                gamma = Formula.Conjunction(new List<Formula> { Formula.Implies(goalAtLeft, fAt1),
                                                                        Formula.Implies(goalLeftAndRight, Formula.Or(fAt1, fAtn)),
                                                                        Formula.Implies(goalAtRight, fAtn) });
            }
            else
            {
                // common knowledge not directly implemented. Modelling common knowledge as 3rd degree knowledge, since there are 3 worlds in this task
                Formula formula = Formula.Conjunction(new List<Formula> { Formula.Implies(goalAtLeft, fAt1),
                                                                        Formula.Implies(goalLeftAndRight, Formula.Or(fAt1, fAtn)),
                                                                        Formula.Implies(goalAtRight, fAtn) });

                gamma = Formula.And(Formula.Knows(agentL, Formula.Knows(agentR, Formula.Knows(agentL, formula))),
                                    Formula.Knows(agentR, Formula.Knows(agentL, Formula.Knows(agentR, formula))));
            }
            // Agents
            Dictionary<string, Agent> agentDict = new Dictionary<string, Agent> { { agentL.name, agentL }, { agentR.name, agentR } };

            // Planning Task
            PlanningTask task = new PlanningTask(initialState, actions, gamma, agentDict, PredicateRepository);
            task.startingLeverPosition = start;
            task.numberOfLeverPositions = n;
            return task;
        }

        public static Predicate CurrentLeverPosition(World w)
        {
            int atProps = w.predicates.Where(p => p.name.StartsWith("at")).Count();
            if (atProps != 1 )
            {
                throw new Exception($"Found {atProps} that are true in this world");
            }
            return w.predicates.First(p => p.name.StartsWith("at"));
        }

        public static Formula PreconditionDelegateMoveRight(World w, PlanningTask task)
        {
            return Formula.And(Formula.Not(Formula.Atom(task.Predicates.Get("atn"))), Formula.Atom(CurrentLeverPosition(w)));
        }

        public static IDictionary<Predicate, bool>? PostconditionDelegateMoveRight(World w, PlanningTask task)
        {
            Predicate currentPositionPredicate = CurrentLeverPosition(w);
            if (currentPositionPredicate.name.Equals("atn"))
            {
                return null;
            }
            int currentPos = int.Parse(currentPositionPredicate.name.Substring(2));
            Predicate delta;
            if (currentPos == task.numberOfLeverPositions - 1)
            {
                delta = task.Predicates.Get("atn");
            }
            else
            {
                delta = task.Predicates.Get("at" + (currentPos + 1).ToString());
            }
            return new Dictionary<Predicate, bool> { { currentPositionPredicate, false }, { delta, true } };
        }
        public static Formula PreconditionDelegateMoveLeft(World w, PlanningTask task)
        {
            return Formula.And(Formula.Not(Formula.Atom(task.Predicates.Get("at1"))), Formula.Atom(CurrentLeverPosition(w)));
        }

        public static IDictionary<Predicate, bool>? PostconditionDelegateMoveLeft(World w, PlanningTask task)
        {
            Predicate currentPositionPredicate = CurrentLeverPosition(w);
            int currentPos;
            if (currentPositionPredicate.name.Equals("atn"))
            {
                currentPos = task.numberOfLeverPositions;
            }
            else
            {
                currentPos = int.Parse(currentPositionPredicate.name.Substring(2));
            }
            if (currentPos == 1)
            {
                return null;
            }
            Predicate delta = task.Predicates.Get("at" + (currentPos - 1).ToString());
            return new Dictionary<Predicate, bool> { { currentPositionPredicate, false }, { delta, true } };
        }
    }
}


