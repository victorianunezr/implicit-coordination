using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public static class PlanningTaskInitializer
    {
        public static PlanningTask DiamondHeist()
        {
            // Agents
            Agent agent0 = new Agent("a0");
            Agent agent1 = new Agent("a1");
            HashSet<Agent> agents = new HashSet<Agent>() { agent0, agent1 };

            // Propositions
            Proposition r = new Proposition("r");
            Proposition l = new Proposition("l");
            Proposition c = new Proposition("c");
            Proposition h = new Proposition("h");

            // Initial state
            World w1 = new World();
            w1.AddProposition(r);
            w1.AddProposition(l);

            World w2 = new World();
            w2.AddProposition(l);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld>() { w1, w2 });
            R.AddEdge(agent1, (w1, w2));

            State initialState = new State(new HashSet<IWorld>() { w1, w2 }, new HashSet<IWorld>() { w1 }, R);

            // Actions
            Event cutRedE1 = new Event(Formula.Atom(r), new Dictionary<ushort, bool>() { { l.id, false } });
            Event cutRedE2 = new Event(Formula.Not(Formula.Atom(r)));

            Action cutRed = new Action(new HashSet<IWorld>() { cutRedE1, cutRedE2},
                                       new HashSet<IWorld>() { cutRedE1 },
                                       agents, "cutRed", agent0);
            cutRed.accessibility.AddEdge(agent1, (cutRedE1, cutRedE2));

            Event cutBlueE1 = new Event(Formula.Not(Formula.Atom(r)), new Dictionary<ushort, bool>() { { l.id, false } });
            Event cutBlueE2 = new Event(Formula.Atom(r));

            Action cutBlue = new Action(new HashSet<IWorld>() { cutBlueE1, cutBlueE2 },
                                        new HashSet<IWorld>() { cutBlueE1 },
                                        agents, "cutBlue", agent0);
            cutBlue.accessibility.AddEdge(agent1, (cutBlueE1, cutBlueE2));

            Event takeE1 = new Event(Formula.Not(Formula.Atom(l)), new Dictionary<ushort, bool> { { h.id, true } });
            Event takeE2 = new Event(Formula.Atom(l), new Dictionary<ushort, bool> { { c.id, true } });

            Action take = new Action(new HashSet<IWorld> { takeE1, takeE2 },
                                     new HashSet<IWorld> { takeE1, takeE2 },
                                     agents, "takeDiamond", agent1);
            take.accessibility.AddEdge(agent0, (takeE1, takeE2));
            take.accessibility.AddEdge(agent1, (takeE1, takeE2));

            HashSet<Action> actions = new HashSet<Action>()
                                    { cutRed, cutBlue, take };

            // Goal formula
            Formula goalFormula = Formula.Atom(h);

            // Agents
            Dictionary<string, Agent> agentDict = new Dictionary<string, Agent> { { agent0.name, agent0 }, { agent1.name, agent1 } };

            // Create planning task
            return new PlanningTask(initialState, actions, goalFormula, agentDict);
        }

        public static PlanningTask SymmetricLever()
        {
            // Agents
            Agent agentL = new Agent("agentLeft");
            Agent agentR = new Agent("agentRight");
            HashSet<Agent> agents = new HashSet<Agent>() { agentL, agentR };

            // Propositions
            Proposition at1 = new Proposition("at1");
            Proposition at2 = new Proposition("at2");
            Proposition at3 = new Proposition("at3");
            Proposition at4 = new Proposition("at4");
            Proposition at5 = new Proposition("at5");
            Proposition goalAt1 = new Proposition("goalAt1");
            Proposition goalAt5 = new Proposition("goalAt5");

            // Initial State
            World w1 = new World();
            w1.AddProposition(at3);
            w1.AddProposition(goalAt1);

            World w2 = new World();
            w2.AddProposition(at3);
            w2.AddProposition(goalAt1);
            w2.AddProposition(goalAt5);

            World w3 = new World();
            w3.AddProposition(at3);
            w3.AddProposition(goalAt5);

            AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w1, w2, w3 });
            R.AddEdge(agentL, (w1, w2));
            R.AddEdge(agentR, (w2, w3));
            State initialState = new State(new HashSet<IWorld> { w1, w2, w3 }, new HashSet<IWorld> { w2 }, R);

            // Actions
            Event right12 = new Event(Formula.And(Formula.Not(Formula.Atom(at5)), Formula.Atom(at1)), new Dictionary<Proposition, bool> { { at1, false }, { at2, true } });
            AccessibilityRelation R12 = new AccessibilityRelation(agents, new HashSet<IWorld> { right12 });
            Action moveRight12 = new Action(new HashSet<IWorld> { right12 }, new HashSet<IWorld> { right12 }, R12, "R12", agentR);

            Event right23 = new Event(Formula.And(Formula.Not(Formula.Atom(at5)), Formula.Atom(at2)), new Dictionary<Proposition, bool> { { at2, false }, { at3, true } });
            AccessibilityRelation R23 = new AccessibilityRelation(agents, new HashSet<IWorld> { right23 });
            Action moveRight23 = new Action(new HashSet<IWorld> { right23 }, new HashSet<IWorld> { right23 }, R23, "R23", agentR);

            Event right34 = new Event(Formula.And(Formula.Not(Formula.Atom(at5)), Formula.Atom(at3)), new Dictionary<Proposition, bool> { { at3, false }, { at4, true } });
            AccessibilityRelation R34 = new AccessibilityRelation(agents, new HashSet<IWorld> { right34 });
            Action moveRight34 = new Action(new HashSet<IWorld> { right34 }, new HashSet<IWorld> { right34 }, R34, "R34", agentR);

            Event right45 = new Event(Formula.And(Formula.Not(Formula.Atom(at5)), Formula.Atom(at4)), new Dictionary<Proposition, bool> { { at4, false }, { at5, true } });
            AccessibilityRelation R45 = new AccessibilityRelation(agents, new HashSet<IWorld> { right45 });
            Action moveRight45 = new Action(new HashSet<IWorld> { right45 }, new HashSet<IWorld> { right45 }, R45, "R45", agentR);

            Event left54 = new Event(Formula.And(Formula.Not(Formula.Atom(at1)), Formula.Atom(at5)), new Dictionary<Proposition, bool> { { at5, false }, { at4, true } });
            AccessibilityRelation L54 = new AccessibilityRelation(agents, new HashSet<IWorld> { left54 });
            Action moveLeft54 = new Action(new HashSet<IWorld> { left54 }, new HashSet<IWorld> { left54 }, L54, "L54", agentL);

            Event left43 = new Event(Formula.And(Formula.Not(Formula.Atom(at1)), Formula.Atom(at4)), new Dictionary<Proposition, bool> { { at4, false }, { at3, true } });
            AccessibilityRelation L43 = new AccessibilityRelation(agents, new HashSet<IWorld> { left43 });
            Action moveLeft43 = new Action(new HashSet<IWorld> { left43 }, new HashSet<IWorld> { left43 }, L43, "L43", agentL);

            Event left32 = new Event(Formula.And(Formula.Not(Formula.Atom(at1)), Formula.Atom(at3)), new Dictionary<Proposition, bool> { { at3, false }, { at2, true } });
            AccessibilityRelation L32 = new AccessibilityRelation(agents, new HashSet<IWorld> { left32 });
            Action moveLeft32 = new Action(new HashSet<IWorld> { left32 }, new HashSet<IWorld> { left32 }, L32, "L32", agentL);

            Event left21 = new Event(Formula.And(Formula.Not(Formula.Atom(at1)), Formula.Atom(at2)), new Dictionary<Proposition, bool> { { at2, false }, { at1, true } });
            AccessibilityRelation L21 = new AccessibilityRelation(agents, new HashSet<IWorld> { left21 });
            Action moveLeft21 = new Action(new HashSet<IWorld> { left21 }, new HashSet<IWorld> { left21 }, L21, "L21", agentL);

            HashSet<Action> actions = new HashSet<Action> { moveLeft21, moveLeft32, moveLeft43, moveLeft54, moveRight12, moveRight23, moveRight34, moveRight45 };

            // Goal Formula
            Formula gamma = Formula.Or(Formula.Atom(at1), Formula.Atom(at5));

            // Agents
            Dictionary<string, Agent> agentDict = new Dictionary<string, Agent> { { agentL.name, agentL }, { agentR.name, agentR } };

            // Planning Task
            return new PlanningTask(initialState, actions, gamma, agentDict);
        }
    }
}
