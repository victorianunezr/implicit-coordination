using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class PlanningTask
    {
        public readonly State initialState;

        public readonly ICollection<Action> actions;
        public readonly Formula goalFormula;

        public PlanningTask(State initialState, HashSet<Action> actions, Formula goalFormula)
        {
            this.initialState = initialState;
            this.actions = actions;
            this.goalFormula = goalFormula;
        }
    }
}
