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
using System.Globalization;

namespace SpagettiMetoden
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string tagId = (args[0]);
                double dayInc = double.Parse(args[1].Replace(",", "."));
                int releasedFish = int.Parse(args[2]);
                double tempDelta = double.Parse(args[3].Replace(",", "."));
                int DepthDelta = int.Parse(args[4]);
                double Increment = double.Parse(args[5].Replace(",", "."));
                double Probability = double.Parse(args[6].Replace(",", "."));
                int iterations = int.Parse(args[7]);
                int algorithm = int.Parse(args[8]);

                Console.WriteLine("dayInc: {0}, releasedFish: {1}, tempDelta: {2}, depthDelta: {3}, Increment: {4}, Propability: {5}, iterations: {6}, algorithms: {7}, tagId: {8}",
                    dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations, algorithm, tagId);

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
                    Controller controller = new Controller(dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations, tagId);
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
                } else
                {
                    Console.WriteLine("Kjører nr 3");
                    ControllerReleaseSteadily controller = new ControllerReleaseSteadily(dayInc, releasedFish, tempDelta, DepthDelta, Increment, Probability, iterations);
                    controller.RunAlgorithm();
                }
                 

                watch.Stop();
                double elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Hvor lang tid tok programmet: {0} sekunder.", elapsedMs / 1000);
                //controller.RunAlgorithm();

                Console.ReadLine();
            } catch {
                /*
                double norLon = 50.236;
                double norLat = 88.642;

                double barLon = 20.698;
                double barLat = 40.638;
                
                float latDiff = (float)(norLat - barLat);
                if (latDiff < 0)
                {
                    latDiff = latDiff * (-1);
                }

                float lonDiff = (float)(norLon - barLon);
                if (lonDiff < 0)
                {
                    lonDiff = lonDiff * (-1);
                }

                Console.WriteLine(Math.Abs(norLat-barLat) + Math.Abs(norLon - barLon));
                Console.WriteLine(latDiff + lonDiff);
                */

                SimpleGPUAcceleration.startUp();
                /*
                Console.WriteLine("Error, bruker standard");
                //Controller controller = new Controller(1, 10000, 1.2, 30, 0.65, 0.5, 30, "742");
                Controller controller = new Controller(4, 10000, 1.2, 30, 0.65, 0, 30, "1664");
                controller.RunAlgorithm();
                */
                Console.ReadKey();
                
            }
                
        }
    }
}  
   
   