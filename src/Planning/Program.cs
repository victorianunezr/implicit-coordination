using System;
using System.Collections.Generic;
using System.IO;
using ImplicitCoordination.Planning;
using ImplicitCoordination.DEL;
using static ImplicitCoordination.Planning.PolicyExecuter;
using Action = ImplicitCoordination.DEL.Action;
using System.Linq;
using System.Diagnostics;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            TestABM();
        }

        public static void SetupAndRunExperiments(string[] args)
        {
            int n = int.Parse(args[0]);
            int s = int.Parse(args[1]);
            int runs = int.Parse(args[2]);
            bool baseline = bool.Parse(args[3]);
            bool writeToFile = bool.Parse(args[3]);

            FileStream ostrm = null;
            StreamWriter writer = null;
            TextWriter oldOut = Console.Out;


            string plannerName = baseline ? "baseline" : "forwardinduction";
            string filename = plannerName + n.ToString() + $"_{runs}runs";
 
            if (writeToFile)
            {
                try
                {
                    ostrm = new FileStream($"../experiments/symmetric_lever/{filename}.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    writer = new StreamWriter (ostrm);
                }
                    catch (Exception e)
                {
                    Console.WriteLine ($"Cannot open {filename}.txt for writing");
                    Console.WriteLine (e.Message);
                    return;
                }
            Console.SetOut(writer);
            }

            RunExperiments(n, s, runs, baseline);

            if (writer != null && ostrm != null)
            {
                Console.SetOut (oldOut);
                writer.Close();
                ostrm.Close();
            }
            Console.WriteLine ("Done");
        }

        public static void RunExperiments(int numberOfPosition, int startingPosition, int numberOfRuns, bool baseline)
        {
            Console.WriteLine($"Number of positions: {numberOfPosition}. Starting position: {startingPosition}. \n");
            
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

            // Console.WriteLine($"Average execution length: {((float)executionLengths.Sum()/(float)executionLengths.Count):F2}");
        }

            public static void TestABM()
            {
                FileStream ostrm = null;
                StreamWriter writer = null;
                TextWriter oldOut = Console.Out;
    
                bool writeToFile = true;

                if (writeToFile)
                {
                    DateTime now = DateTime.Now;
                    string stamp = "_" + now.Day.ToString() + "_" + now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString();
                    string filename = "ABMtest" + stamp;
                    try
                    {
                        ostrm = new FileStream($"../experiments/ABM/{filename}.txt", FileMode.OpenOrCreate, FileAccess.Write);
                        writer = new StreamWriter (ostrm);
                    }
                        catch (Exception e)
                    {
                        Console.WriteLine ($"Cannot open {filename}.txt for writing");
                        Console.WriteLine (e.Message);
                        return;
                    }
                    Console.SetOut(writer);
                }

                Agent a = new Agent("a");
                HashSet<Agent> agents = new HashSet<Agent>() { a };

                World w = new World();
                // Init propositions for lever position
                PropositionRepository propositionRepository = new PropositionRepository();
                for (int i = 0; i<10; i++)
                {
                    Proposition p = new Proposition("p" + i.ToString());
                    propositionRepository.Add(p);
                    w.AddProposition(p);
                }

                AccessibilityRelation R = new AccessibilityRelation(agents, new HashSet<IWorld> { w });
                State initialState = new State(new HashSet<IWorld> { w }, new HashSet<IWorld> { w }, R);

                Event e1 = new Event(Formula.Top());
                Event e2 = new Event(Formula.Top());
                AccessibilityRelation Q = new AccessibilityRelation(agents, new HashSet<IWorld> { e1, e2 });
                Q.AddEdge(a, (e1, e2));

                Action action = new Action(new HashSet<IWorld> { e1, e2 }, new HashSet<IWorld> { e1, e2 } , Q, "action1", a);

                Event e3 = new Event(Formula.Top());
                AccessibilityRelation Q2 = new AccessibilityRelation(agents, new HashSet<IWorld> { e3 });

                Action action2 = new Action(new HashSet<IWorld> { e3 }, new HashSet<IWorld> { e3 } , Q, "action2", a);

                State s = initialState;
                Stopwatch watch = new Stopwatch();
                int noOfUpdates = 10;
                Console.WriteLine($"Updating 1-state Kripke model with 2-event action model {noOfUpdates} times. Starting stopwatch.");
                watch.Start();

                for (int i=0; i<noOfUpdates; i++)
                {
                    s = s.ProductUpdate(action);
                }

                watch.Stop();
                Console.WriteLine($"Elapsed tim after {noOfUpdates} updates: {watch.Elapsed}");
                Console.WriteLine($"Number of states in Kripke model: {s.possibleWorlds.Count}");
                Console.WriteLine("Resetting watch.");

                IList<int> elapsed = new List<int>();

                int noOfUpdatesSmallAction = 25;
                for (int j=1; j<11; j++)
                {
                    watch.Reset();

                    Console.WriteLine($"\nUpdating resulting Kripke model with 1-event action model {noOfUpdatesSmallAction*j} times. Restarting stopwatch.");

                    watch.Start();
                    for (int i=0; i<noOfUpdatesSmallAction*j; i++)
                    {
                        s = s.ProductUpdate(action2);
                    }

                    watch.Stop();
                    Console.WriteLine($"Elapsed time after {noOfUpdatesSmallAction} updates: {watch.Elapsed}");
                    Console.WriteLine($"Number of states in Kripke model: {s.possibleWorlds.Count}");
                    Console.WriteLine("Stopping watch.");
                    
                    elapsed.Append(watch.Elapsed.Seconds);
                }

                // Printing list of elapsed seconds as comma-separated values, for easy copying into excel
                Console.WriteLine("\n");
                Console.WriteLine(String.Join(",", elapsed));

                if (writer != null && ostrm != null)
                {
                    Console.SetOut (oldOut);
                    writer.Close();
                    ostrm.Close();
                }
                Console.WriteLine ("Done");

            }
    }


}
