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
        public string FishTag { get; set; }
        public int TagStep { get; set; }
        public double DayIncrement { get; set; }
        public int RemainingFishToBeReleased { get; set; }
        public double TempDelta { get; set; }
        public string[] DateTable { get; set; }


        static readonly object syncObject = new object();

        public void SetDayIncrement(double dayInc)
        {
            DayIncrement = dayInc;
            //144 er incrementet for å hoppe 24 timer/1 dag i merkedage
            //Ganger det med antall dager som skal inkrementeres.
            TagStep = (int)(144 * dayInc);
        }

        

        public ControllerReleaseSteadily(double dayInc, int remainingFishToBeReleased, double tempDelta, int depthDelta,
            double Increment, double Probability, int iterations, string FishTag)
        {
            this.FishTag = FishTag;
            TempDelta = tempDelta;
            RemainingFishToBeReleased = remainingFishToBeReleased;
            SetDayIncrement(dayInc);

            File = new ReadFromFile();

            FishList = new Dictionary<string, Fish>();
            KeyList = new List<string>();

            File.ReadReleaseAndCapture(FishList, KeyList);
            File.ReadTagData(FishList, KeyList);

            GlobalVariables.Probability = Probability;

            HeatMap = new HeatMap();
            EtaXis = new EtaXi[0];
            TempContainer = new TempContainer(FishList["742"].TagDataList, TagStep);
            CalculateCoordinates = new CalculateCoordinates(Increment, depthDelta, dayInc, iterations);
            DateTable = new string[FishList[FishTag].TagDataList.Count / TagStep];
        }

        public void SetDepthDelta(int DepthDelta)
        {
            CalculateCoordinates.SetDepthDelta(DepthDelta);
        }

        public void RunAlgorithm()
        {
            int dayCounter = 0;
            int counter = 1;
            int deadFishCounter = 0;
            int fishToBeReleased = RemainingFishToBeReleased;
            int totalNumberOfFish = RemainingFishToBeReleased;
            bool use_Norkyst = true;
            var watch = Stopwatch.StartNew();
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>(boundedCapacity: RemainingFishToBeReleased);
            Console.WriteLine("Released Fish: {0}", RemainingFishToBeReleased);
            Console.WriteLine("Tagstep: {0}", TagStep);

            for (int i = 0; i < FishList["742"].TagDataList.Count; i += TagStep)
            {
                TempContainer.UpdateTempArray(FishList[FishTag].TagDataList[i].Date);
                Console.WriteLine("I iterasjon: " + i / TagStep);
                Console.WriteLine("Dead Fish: " + deadFishCounter);

                bool chosenPosition;

                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.NorKystLatArray, HeatMap.NorKystLonArray,
                        HeatMap.BarentsSeaLatArray, HeatMap.BarentsSeaLonArray, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon, use_Norkyst);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalculateCoordinates.FindValidPositions(
                            CalculateCoordinates.CalculatePossibleEtaXi(positionData.Eta_rho, positionData.Xi_rho, false, FishList["742"].TagDataList[i].Depth, use_Norkyst),
                        HeatMap.NorKystLatArray, HeatMap.NorKystLonArray, HeatMap.BarentsSeaLatArray, HeatMap.BarentsSeaLonArray, FishList["742"].TagDataList[i], TempContainer, TempDelta, use_Norkyst
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
                            FishRoute fishRoute = new FishRoute("742", true);
                            fishRoute.PositionDataList.Add((new PositionData(releaseLat,
                                releaseLon)));

                            RouteChooser routeChooser = new RouteChooser(FishList["742"].CaptureLat, FishList["742"].CaptureLon, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);
                            
                            
                            while (!addedToPosDataList)
                            {
                                addedToPosDataList = fishRoute.PositionDataList.TryAdd((new PositionData(
                                    validPositionsDataList.ElementAt(j).Lat, validPositionsDataList.ElementAt(j).Lon,
                                    validPositionsDataList.ElementAt(j).Depth, validPositionsDataList.ElementAt(j).Temp, FishList["742"].TagDataList[i].Depth,
                                    FishList["742"].TagDataList[i].Temp, validPositionsDataList.ElementAt(j).Eta_rho, validPositionsDataList.ElementAt(j).Xi_rho)));
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
                    //TempContainer.UpdateTempArray(day);
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
                                    possiblePositionsArray = CalculateCoordinates.CalculatePossibleEtaXi(pData.Eta_rho, pData.Xi_rho, false, FishList["742"].TagDataList[i].Depth, use_Norkyst);
                                    validPositionsDataList =
                                        CalculateCoordinates.FindValidPositions(
                                            possiblePositionsArray,
                                            HeatMap.NorKystLatArray, HeatMap.NorKystLonArray, HeatMap.BarentsSeaLatArray, HeatMap.BarentsSeaLonArray, tagData, TempContainer, TempDelta, use_Norkyst);
                                }
                                


                                if (validPositionsDataList.Count > 0)
                                {
                                    RouteChooser routeChooser =
                                            new RouteChooser(FishList["742"].CaptureLat, FishList["742"].CaptureLon, pData.Lat, pData.Lon);
                                    while (!chosenPosition)
                                    {
                                        randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                        chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                                    }
                                    FishRoute localFishRoute = new FishRoute("742", true)
                                    {
                                        PositionDataList = fishRoute.PositionDataList
                                    };
                                    fishRoute.PositionDataList.Add((new PositionData(
                                        validPositionsDataList.ElementAt(randInt).Lat,
                                        validPositionsDataList.ElementAt(randInt).Lon,
                                        validPositionsDataList.ElementAt(randInt).Depth,
                                        validPositionsDataList.ElementAt(randInt).Temp,
                                        tagData.Depth, tagData.Temp,
                                        validPositionsDataList.ElementAt(randInt).Eta_rho,
                                        validPositionsDataList.ElementAt(randInt).Xi_rho)));
                                    
                                        if (fishToBeReleased > 0 && validPositionsDataList.Count > 1)
                                        {
                                        int newRandInt = 0;
                                        //bool newChosenPosition = false;
                                        for (int j = 0; j < validPositionsDataList.Count; j++)
                                            {
                                            newRandInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                            //newChosenPosition = routeChooser.ChosenRoute(validPositionsDataList, newRandInt);
                                                if (j != randInt)
                                                {
                                                FishRoute tempFishRoute = localFishRoute;
                                                    lock (FishList["742"])
                                                    {
                                                            tempFishRoute.PositionDataList.Add((new PositionData(
                                                                validPositionsDataList.ElementAt(j).Lat,
                                                                validPositionsDataList.ElementAt(j).Lon,
                                                                validPositionsDataList.ElementAt(j).Depth,
                                                                validPositionsDataList.ElementAt(j).Temp,
                                                                tagData.Depth, tagData.Temp,
                                                                validPositionsDataList.ElementAt(j).Eta_rho,
                                                                validPositionsDataList.ElementAt(j).Xi_rho)));
                                                        FishList["742"].FishRouteList.Add(tempFishRoute);
                                                    }
                                                Interlocked.Decrement(ref fishToBeReleased);
                                                }
                                            }
                                            
                                        }


                                } else {
                                    Interlocked.Increment(ref deadFishCounter);
                                    fishRoute.CommitNotAlive();
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
            }

            watch.Stop();
            double elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Hvor lang tid tok programmet: " + elapsedMs);
            var count = 1;
            string folderName = "Uakseptabel";
            var FishData = FishList["742"];
            double captureLat = FishData.CaptureLat;
            double captureLon = FishData.CaptureLon;


            Console.WriteLine("Program runtime: {0} minutes / {1} seconds.", elapsedMs / 60000, elapsedMs / 1000);
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
                    string[] fishData = fishRoute.FromListToString(DateTable);

                    System.IO.File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\\" + folderName + "\\" + fishRoute.Id + "_" + count + ".txt", fishData);
                    count++;
                }
            }

        }

    }
}
