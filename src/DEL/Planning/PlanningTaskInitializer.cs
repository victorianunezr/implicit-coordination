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
            Agent agent0 = new Agent();
            Agent agent1 = new Agent();
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
            Event cutRedE2 = new Event(Formula.Not(Formula.Atom(r)), null);

            Action cutRed = new Action(new HashSet<IWorld>() { cutRedE1, cutRedE2},
                                       new HashSet<IWorld>() { cutRedE1 },
                                       agents, "cutRed", agent0);
            cutRed.accessibility.AddEdge(agent1, (cutRedE1, cutRedE2));

            Event cutBlueE1 = new Event(Formula.Not(Formula.Atom(r)), new Dictionary<ushort, bool>() { { l.id, false } });
            Event cutBlueE2 = new Event(Formula.Atom(r), null);

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

            IDictionary<string, Action> actions = new Dictionary<string, Action>()
                                    { { cutRed.name, cutRed }, { cutBlue.name, cutBlue }, {take.name, take } };

            // Goal formula
            Formula goalFormula = Formula.Atom(h);

            // Create planning task
            return new PlanningTask(initialState, actions, goalFormula);
        }

        //public PlanningTask SymmetricLever()
        //{
        //    // Agents
        //    Agent agent0 = new Agent();
        //    Agent agent1 = new Agent();
        //    HashSet<Agent> agents = new HashSet<Agent>() { agent0, agent1 };

        //    // Propositions
        //    Proposition r = new Proposition("r");
        //    Proposition l = new Proposition("l");
        //    Proposition c = new Proposition("c");
        //    Proposition h = new Proposition("h");

        //}
    }
}
