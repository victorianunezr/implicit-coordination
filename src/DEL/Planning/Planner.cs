using System;
using System.Collections.Generic;
using ImplicitCoordination.DEL;

namespace ImplicitCoordination.Planning
{
    public static class Planner
    {
        private static HashSet<State> AndNodes;
        private static HashSet<State> OrNodes;

        public static void Plan(State initialState, Agent planningAgent)
        {
            
        }

        public static void init(State root)
        {
            AndNodes = new HashSet<State>();
            OrNodes = new HashSet<State>();

            AndNodes.Add(root);
        }

        public static State BuildTree(State initialState, Agent planningAgent)
        {
            return initialState;
        }

        public static void ComputeCosts(State initialState, Agent planningAgent)
        {

        }

        public static void Prune(State initialState)
        {

        }
    }
}
