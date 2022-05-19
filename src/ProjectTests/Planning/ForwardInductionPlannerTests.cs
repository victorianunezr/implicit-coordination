using System;
using ImplicitCoordination.Planning;
using NUnit.Framework;

namespace Planning.Tests
{
    public class ForwardInductionPlannerTests
    {
        // Not really unit tests, but used for debug-testing for now

        [Test]
        public void Plan()
        {
            PlanningTask leverTask = PlanningTaskInitializer.AsymmetricLever();

            var planner = new ForwardInductionPlanner(leverTask);

            Graph g = planner.Plan(leverTask.agents["agentLeft"]);
        }
    }
}
