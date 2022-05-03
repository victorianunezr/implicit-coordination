using System;
using ImplicitCoordination.Planning;
using NUnit.Framework;

namespace Planning.Tests
{
    public class ForwardInductionPlannerTests
    {
        // Not really unit tests, but used for debug-testing

        [Test]
        public void BuildTree()
        {
            PlanningTask leverTask = PlanningTaskInitializer.SymmetricLever();

            var planner = new ForwardInductionPlanner(leverTask);

            Graph g = planner.BuildTree(leverTask.agents["agentLeft"]);
        }
    }
}
