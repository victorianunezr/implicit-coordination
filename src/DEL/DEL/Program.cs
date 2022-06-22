using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ImplicitCoordination.DEL;
using ImplicitCoordination.DEL.utils;
using ImplicitCoordination.Planning;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            PlanningTask leverTask = PlanningTaskInitializer.AsymmetricLever();

            var planner = new ForwardInductionPlanner(leverTask);

            Graph g = planner.Plan(leverTask.agents["agentLeft"]);
        }
    }
}
