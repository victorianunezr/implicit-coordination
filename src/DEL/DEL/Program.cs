using System;
using System.Collections.Generic;
using System.IO;
using ImplicitCoordination.Planning;
using static ImplicitCoordination.Planning.PolicyExecuter;
using Action = ImplicitCoordination.DEL.Action;
using System.Linq;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(args[0]);
            int s = int.Parse(args[1]);
            int runs = int.Parse(args[2]);
            bool baseline = bool.Parse(args[3]);

            RunExperiments(n, s, runs, baseline);
        }

        public static void RunExperiments(int numberOfPosition, int startingPosition, int numberOfRuns, bool baseline)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;


            string plannerName = baseline ? "baseline" : "forwardinduction";
            string filename = plannerName + numberOfPosition.ToString() + $"_{numberOfRuns}runs"+ $"_start{startingPosition}";
 
            try
            {
                ostrm = new FileStream($"../experiments/asymmetric_lever/{filename}.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter (ostrm);
            }
                catch (Exception e)
            {
                Console.WriteLine ($"Cannot open {filename}.txt for writing");
                Console.WriteLine (e.Message);
                return;
            }
            Console.WriteLine($"Number of positions: {numberOfPosition}. Starting position: {startingPosition}. \n");
            
            Console.SetOut(writer);
            Graph policy = null;
            AndOrGraph baselinePolicy = null;

            PlanningTask leverTask;
            if (!baseline)
            {
                leverTask = LeverTaskInnitializer.LeverTask(n:numberOfPosition, start:startingPosition, baseline:false);
                var planner = new ForwardInductionPlanner(leverTask);
                policy = planner.Plan();
            }
            else
            {
                leverTask = LeverTaskInnitializer.LeverTask(n:numberOfPosition, start:startingPosition, baseline:true);
                var planner = new BaselinePlanner(leverTask);
                baselinePolicy = planner.Plan();
            }

            List<int> executionLengths = new List<int>();
            List<Action> execution;

            for (int i=0; i<numberOfRuns; i++)
            {
                Console.WriteLine($"\nExepriment run {i}");
                try
                {
                    if (!baseline)
                    {
                        execution = PolicyExecuter.ExecutePolicy(policy, leverTask);
                    }
                    else
                    {
                        execution = PolicyExecuter.ExecuteBaselinePolicy(baselinePolicy, leverTask);
                    }
                }
                catch (ExecutionFailedException)
                {
                    i--;
                    Console.WriteLine($"Re-running run {i}");
                    continue;
                }
                
                executionLengths.Add(execution.Count);
            }
            Console.WriteLine($"Number of succesful executions: {executionLengths.Count}");
            Console.WriteLine($"Average execution length: {((float)executionLengths.Sum()/(float)executionLengths.Count):F2}");

            Console.SetOut (oldOut);
            writer.Close();
            ostrm.Close();
            Console.WriteLine ("Done");
            Console.WriteLine($"Average execution length: {((float)executionLengths.Sum()/(float)executionLengths.Count):F2}");
        }
    }
}
