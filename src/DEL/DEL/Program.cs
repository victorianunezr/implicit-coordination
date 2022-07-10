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
            PlanningTask leverTask = PlanningTaskInitializer.SymmetricLever();

            var planner = new ForwardInductionPlanner(leverTask);
            var baseline = new BaselinePlanner(leverTask);

            AndOrGraph baselinePolicy = baseline.Plan();
            // Graph policy = planner.Plan(leverTask.agents["agentLeft"]);

            // PolicyExecuter.ExecutePolicy(policy, leverTask);
        }
    }
}
