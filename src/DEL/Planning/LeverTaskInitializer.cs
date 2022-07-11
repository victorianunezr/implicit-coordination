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
        /// <returns></returns>
        public static PlanningTask LeverTask(int n, int start)
        {
            if (start >= n)
            {
                throw new Exception($"Start position must be between 1 and {n}");
            }

            // Agents
            Agent agentL = new Agent("agentLeft");
            Agent agentR = new Agent("agentRight");
            HashSet<Agent> agents = new HashSet<Agent>() { agentL, agentR };

            // Init propositions for lever position
            PropositionRepository propositionRepository = new PropositionRepository();
            for (int i = 1; i<n; i++)
            {
                propositionRepository.Add(new Proposition("at" + i.ToString()));
            }
            propositionRepository.Add(new Proposition("atn"));

            // Propositions for goal configuration
            // Goal at the leftmost position
            Proposition l = new Proposition("l");
            propositionRepository.Add(l);
            // Goal at the rightmost position
            Proposition r = new Proposition("r");
            propositionRepository.Add(r);
            // Goal at both ends of the lever
            Proposition lr = new Proposition("lr");
            propositionRepository.Add(lr);


            // Get start and end positions from proposition repo
            Proposition at1 = propositionRepository.Get("at1");
            Proposition atn = propositionRepository.Get("atn");
            Proposition atStart = propositionRepository.Get("at" + start.ToString());

            // Initial State
            World w1 = new World();
            w1.AddProposition(atStart);
            w1.AddProposition(l);

            World w2 = new World();
            w2.AddProposition(atStart);
            w2.AddProposition(lr);

            World w3 = new World();
            w3.AddProposition(atStart);
            w3.AddProposition(r);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w1, w2, w3 });
            R.AddEdge(agentL, (w1, w2));
            R.AddEdge(agentR, (w2, w3));
            State initialState = new State(new HashSet<IWorld> { w1, w2, w3 }, new HashSet<IWorld> { w2 }, R);

            Func<World, PlanningTask, Formula> precondDelegateRight = (w,t) => PreconditionDelegateMoveRight(w, t);
            Func<World, PlanningTask, IDictionary<Proposition, bool>?> postcondDelegateRight = (w, t) => PostconditionDelegateMoveRight(w, t);

            Func<World, PlanningTask, Formula> precondDelegateLeft = (w, t) => PreconditionDelegateMoveLeft(w, t);
            Func<World, PlanningTask, IDictionary<Proposition, bool>?> postcondDelegateLeft = (w, t) => PostconditionDelegateMoveLeft(w, t);

            // Actions
            Event pullRightEvent = new Event(precondDelegateRight, postcondDelegateRight);
            AccessibilityRelation moveRightRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { pullRightEvent });
            Action pullRight = new Action(new HashSet<IWorld> { pullRightEvent }, new HashSet<IWorld> { pullRightEvent }, moveRightRelation, "Right", agentR);

            Event pullLeftEvent = new Event(precondDelegateLeft, postcondDelegateLeft);
            AccessibilityRelation moveLeftRelation = new AccessibilityRelation(agents, new HashSet<IWorld> { pullLeftEvent });
            Action pullLeft = new Action(new HashSet<IWorld> { pullLeftEvent }, new HashSet<IWorld> { pullLeftEvent }, moveLeftRelation, "Left", agentL);

            HashSet<Action> actions = new HashSet<Action> { pullLeft, pullRight };

            // Atomic formulas
            Formula fAt1 = Formula.Atom(at1);
            Formula fAtn = Formula.Atom(atn);

            Formula goalAtLeft = Formula.Atom(l);
            Formula goalAtRight = Formula.Atom(r);
            Formula goalLeftAndRight = Formula.Atom(lr);

            Formula gamma = Formula.Conjunction(new List<Formula> { Formula.Implies(goalAtLeft, fAt1),
                                                                    Formula.Implies(goalLeftAndRight, Formula.Or(fAt1, fAtn)),
                                                                    Formula.Implies(goalAtRight, fAtn) });

            // Agents
            Dictionary<string, Agent> agentDict = new Dictionary<string, Agent> { { agentL.name, agentL }, { agentR.name, agentR } };

            // Planning Task
            PlanningTask task = new PlanningTask(initialState, actions, gamma, agentDict, propositionRepository);
            task.startingLeverPosition = start;
            task.numberOfLeverPositions = n;
            return task;
        }

        public static Proposition CurrentLeverPosition(World w)
        {
            int atProps = w.TruePropositions.Where(p => p.name.StartsWith("at")).Count();
            if (atProps != 1 )
            {
                throw new Exception($"Found {atProps} that are true in this world");
            }
            return w.TruePropositions.First(p => p.name.StartsWith("at"));
        }

        public static Formula PreconditionDelegateMoveRight(World w, PlanningTask task)
        {
            return Formula.And(Formula.Not(Formula.Atom(task.propositions.Get("atn"))), Formula.Atom(CurrentLeverPosition(w)));
        }

        public static IDictionary<Proposition, bool>? PostconditionDelegateMoveRight(World w, PlanningTask task)
        {
            Proposition currentPositionProposition = CurrentLeverPosition(w);
            if (currentPositionProposition.name.Equals("atn"))
            {
                return null;
            }
            int currentPos = int.Parse(currentPositionProposition.name.Substring(2));
            Proposition delta;
            if (currentPos == task.numberOfLeverPositions - 1)
            {
                delta = task.propositions.Get("atn");
            }
            else
            {
                delta = task.propositions.Get("at" + (currentPos + 1).ToString());
            }
            return new Dictionary<Proposition, bool> { { currentPositionProposition, false }, { delta, true } };
        }
        public static Formula PreconditionDelegateMoveLeft(World w, PlanningTask task)
        {
            return Formula.And(Formula.Not(Formula.Atom(task.propositions.Get("at1"))), Formula.Atom(CurrentLeverPosition(w)));
        }

        public static IDictionary<Proposition, bool>? PostconditionDelegateMoveLeft(World w, PlanningTask task)
        {
            Proposition currentPositionProposition = CurrentLeverPosition(w);
            int currentPos;
            if (currentPositionProposition.name.Equals("atn"))
            {
                currentPos = task.numberOfLeverPositions;
            }
            else
            {
                currentPos = int.Parse(currentPositionProposition.name.Substring(2));
            }
            if (currentPos == 1)
            {
                return null;
            }
            Proposition delta = task.propositions.Get("at" + (currentPos - 1).ToString());
            return new Dictionary<Proposition, bool> { { currentPositionProposition, false }, { delta, true } };
        }
    }
}


