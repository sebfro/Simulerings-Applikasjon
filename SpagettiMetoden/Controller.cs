﻿using Microsoft.Research.Science.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class Controller
    {
        public ReadFromFile File { get; set; }
        Dictionary<string, Fish> FishList { get; set; }
        List<string> KeyList { get; set; }

        HeatMap HeatMap { get; set; }
        EtaXi[] EtaXis { get; set; }
        public TempContainer TempContainer { get; set; }
        public CalculateCoordinates CalculateCoordinates { get; set; }

        public int TagStep { get; set; }
        public double DayIncrement { get; set; }
        public int ReleasedFish { get; set; }
        public double TempDelta { get; set; }
        

        static readonly object syncObject = new object();

        public void SetDayIncrement(double dayInc)
        {
            DayIncrement = dayInc;
            //144 er incrementet for å hoppe 24 timer/1 dag i merkedage
            //Ganger det med antall dager som skal inkrementeres.
            TagStep = (int) (144 * dayInc);
        }

        public Controller(double dayInc, int releasedFish, double tempDelta, int depthDelta, double Increment, double Probability, int iterations)
        {
            TempDelta = tempDelta;
            ReleasedFish = releasedFish;
            SetDayIncrement(dayInc);

            File = new ReadFromFile();

            FishList = new Dictionary<string, Fish>();
            KeyList = new List<string>();

            File.readReleaseAndCapture(FishList, KeyList);
            File.readTagData(FishList, KeyList);

            GlobalVariables.Probability = Probability;

            HeatMap = new HeatMap();
            EtaXis = new EtaXi[0];
            TempContainer = new TempContainer();
            CalculateCoordinates = new CalculateCoordinates(Increment, depthDelta, dayInc, iterations);
            
        }

        public void SetDepthDelta(int DepthDelta)
        {
            CalculateCoordinates.SetDepthDelta(DepthDelta);
        }

        public void RunAlgorithm()
        {
            double day = GlobalVariables.day;
            int counter = 1;
            int deadFishCounter = 0;
            var watch = Stopwatch.StartNew();
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>(boundedCapacity: ReleasedFish);
            Console.WriteLine("Released Fish: {0}", ReleasedFish);
            Console.WriteLine("Tagstep: {0}", TagStep);

            for (int i = 0; i < FishList["742"].TagDataList.Count; i += TagStep)
            {
                
                Console.WriteLine("I iterasjon: " + i / TagStep);
                bool chosenPosition;
                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();

                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.NorKystLatArray, HeatMap.NorKystLonArray,
                        HeatMap.BarentsSeaLatArray, HeatMap.BarentsSeaLonArray, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalculateCoordinates.FindValidPositions(
                            CalculateCoordinates.CalculatePossibleEtaXi(positionData.Eta_rho, positionData.Xi_rho, false, FishList["742"].TagDataList[i].Depth, TempContainer),
                        HeatMap.NorKystLatArray, HeatMap.NorKystLonArray, FishList["742"].TagDataList[i], TempContainer, TempDelta
                            );

                    float releaseLat = (float)FishList["742"].ReleaseLat;
                    float releaseLon = (float)FishList["742"].ReleaseLon;

                    Parallel.For(0, ReleasedFish, (j) =>
                    {
                        
                        chosenPosition = false;
                        bool addedToPosDataList = false;
                        bool addedToFishRoutList = false;

                        if (validPositionsDataList.Count > 0)
                        {
                            FishRoute fishRoute = new FishRoute("742");
                            fishRoute.PositionDataList.Add((new PositionData(releaseLat,
                                releaseLon)));

                            RouteChooser routeChooser = new RouteChooser(releaseLat, releaseLon, FishList["742"]);

                            while (!chosenPosition)
                            {
                                randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                            }

                            while (!addedToPosDataList)
                            {
                                addedToPosDataList = fishRoute.PositionDataList.TryAdd((new PositionData(
                                    validPositionsDataList.ElementAt(randInt).Lat, validPositionsDataList.ElementAt(randInt).Lon,
                                    validPositionsDataList.ElementAt(randInt).Depth, validPositionsDataList.ElementAt(randInt).Temp, FishList["742"].TagDataList[i].Depth,
                                    FishList["742"].TagDataList[i].Temp, validPositionsDataList.ElementAt(randInt).Eta_rho, validPositionsDataList.ElementAt(randInt).Xi_rho)));
                            }

                            while (!addedToFishRoutList)
                            {
                                addedToFishRoutList = FishList["742"].FishRouteList.TryAdd(fishRoute);
                            }
                        } else
                        {
                            Interlocked.Increment(ref deadFishCounter);
                        }
                    });
                    day += DayIncrement;
                }
                else
                {
                    //TempContainer.UpdateTempArray(day);

                    BlockingCollection<FishRoute> fishRoutes = FishList["742"].FishRouteList;
                    TagData tagData = FishList["742"].TagDataList[i];
                    if (deadFishCounter < ReleasedFish)
                    {
                        
                        Parallel.ForEach(fishRoutes, (fishRoute) =>
                        {
                            int randInt = 0;
                            chosenPosition = false;
                            EtaXi[] possiblePositionsArray;
                            BlockingCollection<PositionData> validPositionsDataList;
                            if (fishRoute.Alive)
                            {
                                PositionData pData = fishRoute.PositionDataList.ElementAt(counter);

                                lock (syncObject)
                                {
                                    possiblePositionsArray = CalculateCoordinates.CalculatePossibleEtaXi(pData.Eta_rho, pData.Xi_rho, Math.Abs(pData.Depth - tagData.Depth) < 30, tagData.Depth, TempContainer);
                                    validPositionsDataList =
                                        CalculateCoordinates.FindValidPositions(
                                            possiblePositionsArray,
                                            HeatMap.NorKystLatArray, HeatMap.NorKystLonArray, tagData, TempContainer, TempDelta);
                                }

                                

                                if (validPositionsDataList.Count > 0)
                                {
                                    RouteChooser routeChooser =
                                            new RouteChooser(pData.Lat, pData.Lon, FishList["742"]);
                                        while (!chosenPosition)
                                        {
                                            randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                            chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                                        }
                                    fishRoute.PositionDataList.Add((new PositionData(
                                            validPositionsDataList.ElementAt(randInt).Lat,
                                            validPositionsDataList.ElementAt(randInt).Lon,
                                            validPositionsDataList.ElementAt(randInt).Depth,
                                            validPositionsDataList.ElementAt(randInt).Temp,
                                            tagData.Depth, tagData.Temp,
                                            validPositionsDataList.ElementAt(randInt).Eta_rho,
                                            validPositionsDataList.ElementAt(randInt).Xi_rho)));
                                    
                                }
                                else
                                {
                                    Interlocked.Increment(ref deadFishCounter);
                                    fishRoute.CommitNotAlive();
                                    /*Console.WriteLine("I iterasjon: " + i / GlobalVariables.tagStep + " ELIMINERT");
                                    Console.WriteLine("eta: " + pData.eta_rho + ", xi: " + pData.xi_rho);
                                    Console.WriteLine("dybde: " + tagData.depth + ", temp: " + tagData.temp);
                                    Console.WriteLine("dybde: " + pData.depth + ", temp: " + pData.temp);
                                    */
                                }
                            }
                        });
                    }
                    else
                    {
                        i = FishList["742"].TagDataList.Count;
                    }
                    counter++;
                }

                TempContainer.UpdateTempArray(FishList["742"].TagDataList[i].Date);
                /*day += DayIncrement;
                if ((Math.Abs(day % 1) <= (double.Epsilon * 100)) && (day <= 225))
                {
                    try
                    {
                        TempContainer.UpdateTempArray(day);
                    }
                    catch (KeyNotFoundException exception)
                    {
                        Console.WriteLine("DayOutOfRange: {0}", exception);
                    }
                }*/
            }

            watch.Stop();
            double elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Hvor lang tid tok programmet: " + elapsedMs);
            var count = 1;
            string folderName = "Uakseptabel";
            var FishData = FishList["742"];
            double captureLat = FishData.CaptureLat;
            double captureLon = FishData.CaptureLon;


            Console.WriteLine("Hvor lang tid tok programmet: {0} minutter.", elapsedMs / 60000);
            Console.WriteLine("Hvor lang tid tok programmet: {0} sekunder.", elapsedMs / 1000);
            Console.WriteLine("Fishlist count: {0}", FishList["742"].FishRouteList.Count);
            Console.WriteLine("Dead fish counter: {0}", deadFishCounter);
            Console.WriteLine("Alive fish counter: {0}", ReleasedFish - deadFishCounter);
            if (deadFishCounter == ReleasedFish)
            {
                Console.WriteLine("All fish are dead");

            }
            else
            {
                //SLETTER ALLE FILER I FOLDER AKSEPTABEL OG UAKSEPTABEL !!!!!!!!!!!!!!!!!! Lag backup folder om tester hjemme eller HI
                DirectoryInfo di = new DirectoryInfo(@"C:\NCdata\fishData\Akseptabel\");
                DirectoryInfo di2 = new DirectoryInfo(@"C:\NCdata\fishData\Uakseptabel\");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (FileInfo file in di2.GetFiles())
                {
                    file.Delete();
                }
            }
            Console.WriteLine("Day is: {0}", day);

            foreach (var fishRoute in FishList["742"].FishRouteList)
            {
                //Console.WriteLine("Is fish alive?: " + fishRoute.alive);
                if (fishRoute.Alive)
                {
                    var posData = fishRoute.PositionDataList.ElementAt(fishRoute.PositionDataList.Count - 1);
                    if (CalculateCoordinates.GetDistanceFromLatLonInKm(posData.Lat, posData.Lon, captureLat, captureLon) <
                        ((CalculateCoordinates.Increment * 3.6) * (DayIncrement * 24)))
                    {
                        folderName = "Akseptabel";
                    } else
                    {
                        folderName = "Uakseptabel";
                    }
                    string[] fishData = fishRoute.FromListToString();

                    System.IO.File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\\" + folderName + "\\" + fishRoute.Id + "_" + count + ".txt", fishData);
                    count++;
                }
            }
            
        }
    }
}
