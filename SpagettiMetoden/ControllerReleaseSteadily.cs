using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class ControllerReleaseSteadily
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
        public int RemainingFishToBeReleased { get; set; }
        public double TempDelta { get; set; }


        static readonly object syncObject = new object();

        public void SetDayIncrement(double dayInc)
        {
            DayIncrement = dayInc;
            //144 er incrementet for å hoppe 24 timer/1 dag i merkedage
            //Ganger det med antall dager som skal inkrementeres.
            TagStep = (int)(144 * dayInc);
        }

        

        public ControllerReleaseSteadily(double dayInc, int remainingFishToBeReleased, double tempDelta, int depthDelta,
            double Increment, double Probability, int iterations)
        {
            TempDelta = tempDelta;
            RemainingFishToBeReleased = remainingFishToBeReleased;
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
            int dayCounter = 0;
            int day = GlobalVariables.day;
            int counter = 1;
            int deadFishCounter = 0;
            int fishToBeReleased = RemainingFishToBeReleased;
            int totalNumberOfFish = RemainingFishToBeReleased;
            var watch = Stopwatch.StartNew();
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>(boundedCapacity: RemainingFishToBeReleased);
            Console.WriteLine("Released Fish: {0}", RemainingFishToBeReleased);
            Console.WriteLine("Tagstep: {0}", TagStep);

            for (int i = 0; i < FishList["742"].TagDataList.Count; i += TagStep)
            {

                Console.WriteLine("I iterasjon: " + i / TagStep);
                bool chosenPosition;

                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();
                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.LatArray, HeatMap.LonArray, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalculateCoordinates.FindValidPositions(
                            CalculateCoordinates.CalculatePossibleEtaXi(positionData.Eta_rho, positionData.Xi_rho, false, FishList["742"].TagDataList[i].depth, TempContainer),
                        HeatMap.LatArray, HeatMap.LonArray, FishList["742"].TagDataList[i], TempContainer, TempDelta
                            );

                    float releaseLat = (float)FishList["742"].ReleaseLat;
                    float releaseLon = (float)FishList["742"].ReleaseLon;

                    int releasedFish = validPositionsDataList.Count;
                    Interlocked.Add(ref fishToBeReleased, (releasedFish * (-1)));

                    Parallel.For(0, releasedFish, (j) =>
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
                            
                            
                            while (!addedToPosDataList)
                            {
                                addedToPosDataList = fishRoute.PositionDataList.TryAdd((new PositionData(
                                    validPositionsDataList.ElementAt(j).Lat, validPositionsDataList.ElementAt(j).Lon,
                                    validPositionsDataList.ElementAt(j).Depth, validPositionsDataList.ElementAt(j).Temp, FishList["742"].TagDataList[i].depth,
                                    FishList["742"].TagDataList[i].temp, validPositionsDataList.ElementAt(j).Eta_rho, validPositionsDataList.ElementAt(j).Xi_rho)));
                            }

                            while (!addedToFishRoutList)
                            {
                                addedToFishRoutList = FishList["742"].FishRouteList.TryAdd(fishRoute);
                            }
                        }
                        else
                        {
                            Interlocked.Increment(ref deadFishCounter);
                        }
                    });
                    dayCounter++;
                }
                else
                {
                    TempContainer.UpdateTempArray(day);
                    BlockingCollection<FishRoute> fishRoutes = FishList["742"].FishRouteList;
                    TagData tagData = FishList["742"].TagDataList[i];

                    if (deadFishCounter < FishList["742"].FishRouteList.Count)
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
                                    possiblePositionsArray = CalculateCoordinates.CalculatePossibleEtaXi(pData.Eta_rho, pData.Xi_rho, false, FishList["742"].TagDataList[i].depth, TempContainer);
                                    validPositionsDataList =
                                        CalculateCoordinates.FindValidPositions(
                                            possiblePositionsArray,
                                            HeatMap.LatArray, HeatMap.LonArray, tagData, TempContainer, TempDelta);
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
                                        tagData.depth, tagData.temp,
                                        validPositionsDataList.ElementAt(randInt).Eta_rho,
                                        validPositionsDataList.ElementAt(randInt).Xi_rho)));
                                    
                                        if (fishToBeReleased > 0 && validPositionsDataList.Count > 1)
                                        {
                                        int newRandInt = 0;
                                        bool newChosenPosition = false;
                                        for (int j = 0; j < validPositionsDataList.Count; j++)
                                            {
                                            newRandInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                            newChosenPosition = routeChooser.ChosenRoute(validPositionsDataList, newRandInt);
                                                if (j != randInt && newChosenPosition)
                                                {
                                                    FishRoute tempFishRoute = new FishRoute("742")
                                                    {
                                                        PositionDataList = fishRoute.PositionDataList
                                                    };
                                                    lock (FishList["742"])
                                                    {
                                                            tempFishRoute.PositionDataList.Add((new PositionData(
                                                                validPositionsDataList.ElementAt(j).Lat,
                                                                validPositionsDataList.ElementAt(j).Lon,
                                                                validPositionsDataList.ElementAt(j).Depth,
                                                                validPositionsDataList.ElementAt(j).Temp,
                                                                tagData.depth, tagData.temp,
                                                                validPositionsDataList.ElementAt(j).Eta_rho,
                                                                validPositionsDataList.ElementAt(j).Xi_rho)));
                                                        FishList["742"].FishRouteList.Add(tempFishRoute);
                                                    }
                                                Interlocked.Decrement(ref fishToBeReleased);
                                                }
                                            }
                                            
                                            //Interlocked.Add(ref fishToBeReleased, (Interlocked.Decrement(ref releasedFish)) * (-1));
                                        }


                                } else {
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

                    dayCounter++;
                    counter++;
                }

                if (dayCounter == 2 && DayIncrement < 1)
                {
                    dayCounter = 0;
                    day++;
                }
                else if (DayIncrement >= 1)
                {
                    day += (int)DayIncrement;
                }
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
            Console.WriteLine("Alive fish counter: {0}", totalNumberOfFish - deadFishCounter);
            Console.WriteLine("totalnumberoffish: {0}", totalNumberOfFish);
            Console.WriteLine("RemainingFish: {0}", fishToBeReleased);
            if (deadFishCounter == totalNumberOfFish)
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
                    if (CalculateCoordinates.GetDistanceFromLatLonInKm(posData.Lat, posData.Lon, captureLat, captureLon) < CalculateCoordinates.Increment)
                    {
                        folderName = "Akseptabel";
                    }
                    else
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
