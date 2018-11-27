using Microsoft.Research.Science.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics.Statistics;

namespace SpagettiMetoden
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int tagId = int.Parse(args[0]);
                double dayInc = double.Parse(args[1].Replace(".", ","));
                int releasedFish = int.Parse(args[2]);
                double tempDelta = double.Parse(args[3].Replace(".", ","));
                int DepthDelta = int.Parse(args[4]);
                double Increment = double.Parse(args[5].Replace(".", ","));
                double Probability = double.Parse(args[6].Replace(".", ","));
                int iterations = int.Parse(args[7]);
                int algorithm = int.Parse(args[8]);


                Console.WriteLine("Increment: {0}, Probability: {1}, possible pos: {2}", Increment, Probability, iterations);
                //string answer = "";

                
                


                //Skriv til setup fil
                string[] setup = { "ID:\t Days:\t Numb of fish\t Temp delta:\t Depth delta:\t Increment 1: \t Increment 2:\t", args[0] + "\t" + args[1] + "\t" + args[2] + "\t"
                                                                                                                           + args[3] + "\t" + args[4] + "\t" + args[5] + "\t" + args[6] + "\t" + args[7]};

                File.WriteAllLines(@"C:\NCdata\fishData\setup.txt", setup);

                Console.WriteLine("Running algorithm...");
                var watch = Stopwatch.StartNew();
                bool failed = false;
                //controller = new Controller(dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations);
                if(algorithm == 0)
                {
                    Controller controller = new Controller(dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations);
                    controller.RunAlgorithm();
                } else if (algorithm == 1)
                {
                    ControllerReleaseFwAndBw controller = new ControllerReleaseFwAndBw(dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations);

                    if (controller.RunAlgorithmFW())
                    {
                        if (controller.RunAlgorithmBW())
                        {
                            Merge.MergeFwAndBwFiles(Increment, dayInc);
                        }
                        else
                        {
                            failed = true;
                        }
                    }
                    else
                    {
                        failed = true;
                    }

                    if (failed)
                    {
                        Console.WriteLine("It's a failure like torkel");
                    }
                }
                 

                watch.Stop();
                double elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Hvor lang tid tok programmet: {0} sekunder.", elapsedMs / 1000);
                //controller.RunAlgorithm();

                Console.ReadLine();
            } catch {
                ConsoleUI consoleUI = new ConsoleUI();

                consoleUI.RunUI();
            }
                
        }
    }
}  
   
   