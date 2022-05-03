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

        /// <summary>
        /// Dictionary mapping agent names to agents, used to access agent objects from the outside.
        /// </summary>
        public readonly IDictionary<string, Agent> agents;

        public PlanningTask(State initialState, HashSet<Action> actions, Formula goalFormula, IDictionary<string, Agent> agents)
        {
            this.initialState = initialState;
            this.actions = actions;
            this.goalFormula = goalFormula;
            this.agents = agents;
        }
    }
}
