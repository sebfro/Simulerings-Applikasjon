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
            //bool run = true;

             int tagId = int.Parse(args[0]);
            double dayInc = double.Parse(args[1].Replace(".", ","));
            int releasedFish = int.Parse(args[2]);
            double tempDelta = double.Parse(args[3].Replace(".", ","));
            int DepthDelta = int.Parse(args[4]);
            double Increment = double.Parse(args[5].Replace(".", ","));
            double propability = double.Parse(args[6].Replace(".", ","));
            int iterations = int.Parse(args[7]);

            
            Console.WriteLine("Increment: {0}, propability: {1}, possible pos: {2}", Increment, propability, iterations);
            //string answer = "";
            
            Controller controller;

            

            //Skriv til setup fil
            string[] setup = { "ID:\t Days:\t Numb of fish\t Temp delta:\t Depth delta:\t Increment 1: \t Increment 2:\t", args[0] + "\t" + args[1] + "\t" + args[2] + "\t" 
                                                                                                                           + args[3] + "\t" + args[4] + "\t" + args[5] + "\t" + args[6] + "\t" + args[7]};

            File.WriteAllLines(@"C:\NCdata\fishData\setup.txt", setup);


            /*
            while (run)
            {
                Console.Write("Enter day increment per iterasion: ");
                dayInc = int.Parse(Console.ReadLine());
                Console.WriteLine("The day increment per iterasjon will be {0}.", dayInc);


                Console.Write("Enter how many fish to release:");
                releasedFish = int.Parse(Console.ReadLine());
                Console.WriteLine("{0} fish will be released per iterasjon.", releasedFish);


                try
                {
                    Console.Write("Enter the tempdelta:");
                    tempDelta = double.Parse(Console.ReadLine());
                    Console.WriteLine("The tempDelta will be {0}.", tempDelta);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Use a comma, not a dot! This expcetion was thrown: {0}", ex);
                }

                Console.Write("Enter Increment:");
                Increment = int.Parse(Console.ReadLine());
                Console.WriteLine("Increment will be {0}.", Increment);

                Console.Write("Enter the Increment2:");
                Increment2 = int.Parse(Console.ReadLine());
                Console.WriteLine("Increment2 will be {0}.", Increment2);

                Console.Write("Enter the depth delta:");
                DepthDelta = int.Parse(Console.ReadLine());
                Console.WriteLine("Depth delta will be {0}.", DepthDelta);


                Console.WriteLine("Loading files..."); */
            Console.WriteLine("Running algorithm...");
                controller = new Controller(dayInc, releasedFish, tempDelta, DepthDelta, Increment, propability, iterations);
                //bool runCurrentConfig = true;
                controller.RunAlgorithm();
            /*while (runCurrentConfig)
            {
                Console.WriteLine("Do you want to rerun the Algorithm with current configuration? (Y/N)");
                answer = Console.ReadLine();
                runCurrentConfig = (answer != "N");

                if (runCurrentConfig)
                {
                    Console.Write("Enter how many fish to release:");
                    releasedFish = int.Parse(Console.ReadLine());
                    Console.WriteLine("{0} fish will be released per iterasjon.", releasedFish);

                    try
                    {
                        Console.Write("Enter the tempdelta:");
                        tempDelta = double.Parse(Console.ReadLine());
                        Console.WriteLine("The tempDelta will be {0}.", tempDelta);
                    } catch(FormatException ex)
                    {
                        Console.WriteLine("Use a comma, not a dot! Thi expcetion was thrown: {0}", ex);
                    }

                    Console.Write("Enter Increment:");
                    Increment = int.Parse(Console.ReadLine());
                    Console.WriteLine("Increment will be {0}.", Increment);

                    Console.Write("Enter the Increment2:");
                    Increment2 = int.Parse(Console.ReadLine());
                    Console.WriteLine("Increment2 will be {0}.", Increment2);

                    Console.Write("Enter the depth delta:");
                    DepthDelta = int.Parse(Console.ReadLine());
                    Console.WriteLine("Depth delta will be {0}.", DepthDelta);

                    controller.TempDelta = tempDelta;
                    controller.ReleasedFish = releasedFish;
                    controller.SetIncrements(Increment, Increment2);
                    controller.SetDepthDelta(DepthDelta);

                    controller.RunAlgorithm();
                }
            }

            Console.WriteLine("Do you want to change the configuration and rerun the Algorithm? (Y/N)");
            answer = Console.ReadLine();
            run = (answer != "N");

        }
        */

            /*
            int deadFishCounter = 0;
            
            ReadFromFile file = new ReadFromFile();

            Dictionary<string, Fish> FishList = new Dictionary<string, Fish>();
            List<string> KeyList = new List<string>();
            DataSet dsOfZ = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NK800_Z.nc");
            Array Z_Array = dsOfZ["Z"].GetData();

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);
           
            int counter = 1;

            HeatMap heatMap = new HeatMap();
            EtaXi[] etaXis = new EtaXi[0];
            int day = GlobalVariables.day;
            CallPython callPython = new CallPython();

            
            /*
            var watch = Stopwatch.StartNew();


            //Har prøvd å endre i fra 500 til GlobalVariables.tagStep
            for (int i = 0; i < FishList["742"].tagDataList.Count; i+=GlobalVariables.tagStep)
            {
                Console.WriteLine("I iterasjon: " + i / GlobalVariables.tagStep);
                bool chosenPosition;

                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();

                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(heatMap.latArray, heatMap.lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(CalcDistance_BetweenTwoLonLatCoordinates.calculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho, heatMap.mask_rhoArray), 
                        heatMap.latArray, heatMap.lonArray, FishList["742"].tagDataList[i], heatMap.depthArray, Z_Array, day, callPython);

                    float releaseLat = (float)FishList["742"].releaseLat;
                    float releaseLon = (float)FishList["742"].releaseLon;

                    Parallel.For(0, GlobalVariables.releasedFish, j =>
                    {
                        chosenPosition = false;

                        if (validPositionsDataList.Count > 0)
                        {
                            FishRoute fishRoute = new FishRoute("742");
                            fishRoute.PositionDataList.Add((new PositionData(releaseLat,
                                releaseLon)));

                            RouteChooser routeChooser = new RouteChooser(releaseLat, releaseLon, FishList["742"]);

                            while (!chosenPosition)
                            {
                                randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                chosenPosition = routeChooser.chosenRoute(validPositionsDataList, randInt);
                            }

                            fishRoute.PositionDataList.Add((new PositionData(
                                    validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                    validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp, FishList["742"].tagDataList[i].depth,
                                    FishList["742"].tagDataList[i].temp, validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));
                            FishList["742"].FishRouteList.Add(fishRoute);
                        }
                    });
                }
                else
                {
                    callPython.updateTempArray(day);
                    BlockingCollection<FishRoute> fishRoutes = FishList["742"].FishRouteList;
                    TagData tagData = FishList["742"].tagDataList[i];
                    if(deadFishCounter < GlobalVariables.releasedFish)
                    {
                        Parallel.ForEach(fishRoutes, (fishRoute) =>
                        {
                            int randInt = 0;
                            chosenPosition = false;

                            if (fishRoute.alive)
                            {
                                PositionData pData = fishRoute.PositionDataList.ElementAt(counter);

                                BlockingCollection<PositionData> validPositionsDataList =
                                    CalcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(CalcDistance_BetweenTwoLonLatCoordinates.calculatePossibleEtaXi(pData.eta_rho,
                                    pData.xi_rho, heatMap.mask_rhoArray), heatMap.latArray, heatMap.lonArray, tagData, heatMap.depthArray, Z_Array, day, callPython);

                                if (validPositionsDataList.Count > 0)
                                {
                                    RouteChooser routeChooser = new RouteChooser(pData.lat, pData.lon, FishList["742"]);
                                    while (!chosenPosition)
                                    {
                                        randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                        chosenPosition = routeChooser.chosenRoute(validPositionsDataList, randInt);
                                    }
                                    fishRoute.PositionDataList.Add((new PositionData(
                                        validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                        validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp,
                                        tagData.depth, tagData.temp,
                                        validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));
                                }

                                else
                                {
                                    Interlocked.Increment(ref deadFishCounter);
                                    fishRoute.commitNotAlive();
                                    /*Console.WriteLine("I iterasjon: " + i / GlobalVariables.tagStep + " ELIMINERT");
                                    Console.WriteLine("eta: " + pData.eta_rho + ", xi: " + pData.xi_rho);
                                    Console.WriteLine("dybde: " + tagData.depth + ", temp: " + tagData.temp);
                                    Console.WriteLine("dybde: " + pData.depth + ", temp: " + pData.temp);
                                    
                                }
                            }
                        });
                    } else
                    {
                        i = FishList["742"].tagDataList.Count;
                    }
                    
                    counter++;
                }
                day += GlobalVariables.dayIncrement;
                

            }

            watch.Stop();
            double elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Hvor lang tid tok programmet: " + elapsedMs);
            var count = 1;
            string folderName = "Uakseptabel";
            var FishData = FishList["742"];
            double captureLat = FishData.captureLat;
            double captureLon = FishData.captureLon;
            foreach (var fishRoute in FishList["742"].FishRouteList)
            {
                //Console.WriteLine("Is fish alive?: " + fishRoute.alive);
                if (fishRoute.alive)
                {
                    var posData = fishRoute.PositionDataList.ElementAt(fishRoute.PositionDataList.Count-1);
                    if(CalcDistance_BetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(posData.lat, posData.lon, captureLat, captureLon) <  GlobalVariables.increment2*4)
                    {
                        folderName = "Akseptabel";
                    }
                    string[] fishData = fishRoute.fromListToString();

                    File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\\" + folderName + "\\" + fishRoute.id +"_" + count + ".txt", fishData);
                    count++;
                }
            }
            Console.WriteLine("Hvor lang tid tok programmet: " + elapsedMs/60000);
            Console.WriteLine("Dead fish counter: {0}", deadFishCounter);
            Console.WriteLine("Alive fish counter: {0}", GlobalVariables.releasedFish - deadFishCounter);
            if (deadFishCounter == GlobalVariables.releasedFish)
            {
                Console.WriteLine("All fish are dead");
            }
            Console.WriteLine("Day is: {0}", day);
            */
            Console.ReadLine();
        }
    }
}  
   
   