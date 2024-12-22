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
        /// List of goal formulas is used in goal recognition tasks, where there is uncertainty about the real goal.
        /// </summary>
        public readonly IList<Formula> listGoalFormulas;

        /// <summary>
        /// Dictionary mapping agent names to agents, used to access agent objects from the outside.
        /// </summary>
        public readonly IDictionary<string, Agent> agents;

        public PredicateRepository Predicates;

        public int numberOfLeverPositions;
        public int startingLeverPosition;

        public PlanningTask(State initialState, HashSet<Action> actions, Formula goalFormula, IDictionary<string, Agent> agents, PredicateRepository Predicates)
        {
            this.initialState = initialState;
            this.actions = actions;
            this.goalFormula = goalFormula;
            this.listGoalFormulas = null;
            this.agents = agents;
            this.Predicates = Predicates;
        }

        public PlanningTask(State initialState, HashSet<Action> actions, IList<Formula> listGoalFormulas, IDictionary<string, Agent> agents, PredicateRepository Predicates)
        {
            this.initialState = initialState;
            this.actions = actions;
            this.listGoalFormulas = listGoalFormulas;
            this.agents = agents;
            this.Predicates = Predicates;
        }

    }
}
