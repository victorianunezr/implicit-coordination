﻿using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;
using Action = ImplicitCoordination.DEL.Action;

namespace ImplicitCoordination.Planning
{
    public class PlanningTask
    {
        public readonly State initialState;
        public readonly IDictionary<string,Action> actions;
        public readonly Formula goalFormula;

        public PlanningTask(State initialState, IDictionary<string, Action> actions, Formula goalFormula)
        {
            this.initialState = initialState;
            this.actions = actions;
            this.goalFormula = goalFormula;
        }
    }
}
