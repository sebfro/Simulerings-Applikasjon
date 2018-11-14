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
    class ControllerReleaseFwAndBw
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
            TagStep = (int)(144 * dayInc);
        }

        public ControllerReleaseFwAndBw(double dayInc, int releasedFish, double tempDelta, int depthDelta, double Increment, double Probability, int iterations)
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

        public void RunAlgorithmFW()
        {
            int dayCounter = 0;
            int day = GlobalVariables.day;
            int counter = 1;
            int deadFishCounter = 0;
            var watch = Stopwatch.StartNew();
            int halfTagDataCount = (FishList["742"].TagDataList.Count / 2);
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>(boundedCapacity: ReleasedFish);

            int halfIterations = halfTagDataCount / TagStep;

            Console.WriteLine("Released Fish: {0}", ReleasedFish);
            Console.WriteLine("Tagstep: {0}", TagStep);


            for (int i = 0; i < halfTagDataCount && halfIterations > 0; i += TagStep)
            {

                Console.WriteLine("I iterasjon: " + i / TagStep);
                bool chosenPosition;
                halfIterations--;

                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();

                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.LatArray, HeatMap.LonArray, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalculateCoordinates.FindValidPositions(
                            CalculateCoordinates.CalculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho, false),
                        HeatMap.LatArray, HeatMap.LonArray, FishList["742"].TagDataList[i], TempContainer, TempDelta
                            );

                    float releaseLat = (float)FishList["742"].ReleaseLat;
                    float releaseLon = (float)FishList["742"].ReleaseLon;

                    Parallel.For(0, ReleasedFish, (j) =>
                    {

                        chosenPosition = false;
                        bool addedToPosDataList = false;
                        bool addedToFishRouteList = false;

                        if (validPositionsDataList.Count > 0)
                        {
                            FishRoute fishRoute = new FishRoute("742");
                            fishRoute.PositionDataList.Add((new PositionData(releaseLat,
                                releaseLon)));

                            RouteChooser routeChooser = new RouteChooser(releaseLat, releaseLon, FishList["742"].CaptureLat, FishList["742"].CaptureLon);

                            while (!chosenPosition)
                            {
                                randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                            }

                            while (!addedToPosDataList)
                            {
                                addedToPosDataList = fishRoute.PositionDataList.TryAdd((new PositionData(
                                    validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                    validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp, FishList["742"].TagDataList[i].depth,
                                    FishList["742"].TagDataList[i].temp, validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));
                            }

                            while (!addedToFishRouteList)
                            {
                                addedToFishRouteList = FishList["742"].FishRouteList.TryAdd(fishRoute);
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
                                    possiblePositionsArray = CalculateCoordinates.CalculatePossibleEtaXi(pData.eta_rho, pData.xi_rho, Math.Abs(pData.depth - tagData.depth) < 30);
                                    validPositionsDataList =
                                        CalculateCoordinates.FindValidPositions(
                                            possiblePositionsArray,
                                            HeatMap.LatArray, HeatMap.LonArray, tagData, TempContainer, TempDelta);
                                }



                                if (validPositionsDataList.Count > 0)
                                {
                                    RouteChooser routeChooser =
                                            new RouteChooser(pData.lat, pData.lon, FishList["742"].CaptureLat, FishList["742"].CaptureLon);
                                    while (!chosenPosition)
                                    {
                                        randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                        chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                                    }
                                    fishRoute.PositionDataList.Add((new PositionData(
                                            validPositionsDataList.ElementAt(randInt).lat,
                                            validPositionsDataList.ElementAt(randInt).lon,
                                            validPositionsDataList.ElementAt(randInt).depth,
                                            validPositionsDataList.ElementAt(randInt).temp,
                                            tagData.depth, tagData.temp,
                                            validPositionsDataList.ElementAt(randInt).eta_rho,
                                            validPositionsDataList.ElementAt(randInt).xi_rho)));

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
                DirectoryInfo di = new DirectoryInfo(GlobalVariables.pathToSaveFishData + @"\FW\");

                foreach (FileInfo file in di.GetFiles())
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
                    string[] fishData = fishRoute.FromListToString();

                    System.IO.File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\FW\" + fishRoute.Id + "_" + count + ".txt", fishData);
                    count++;
                }
            }

        }

        public void RunAlgorithmBW()
        {
            int dayCounter = 0;
            int day = GlobalVariables.lastDay;
            int counter = 1;
            int deadFishCounter = 0;
            var watch = Stopwatch.StartNew();
            int halfTagDataCount = (FishList["742"].TagDataList.Count / 2);
            int tagDataCount = FishList["742"].TagDataList.Count -1;
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>(boundedCapacity: ReleasedFish);

            int halfIterations = halfTagDataCount / TagStep;

            Console.WriteLine("Released Fish: {0}", ReleasedFish);
            Console.WriteLine("Tagstep: {0}", TagStep);

            for (int i = tagDataCount; i > halfTagDataCount && halfIterations > 0; i -= TagStep)
            {

                halfIterations--;
                Console.WriteLine("I iterasjon: " + i / TagStep);
                bool chosenPosition;

                if (i == tagDataCount)
                {
                    var watch2 = Stopwatch.StartNew();

                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.LatArray, HeatMap.LonArray, FishList["742"].CaptureLat, FishList["742"].CaptureLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalculateCoordinates.FindValidPositions(
                            CalculateCoordinates.CalculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho, false),
                        HeatMap.LatArray, HeatMap.LonArray, FishList["742"].TagDataList[i], TempContainer, TempDelta
                            );

                    float captureLat = (float)FishList["742"].CaptureLat;
                    float captureLon = (float)FishList["742"].CaptureLon;

                    Parallel.For(0, ReleasedFish, (j) =>
                    {

                        chosenPosition = false;
                        bool addedToPosDataList = false;
                        bool addedToFishRouteList = false;

                        if (validPositionsDataList.Count > 0)
                        {
                            FishRoute fishRoute = new FishRoute("742");
                            fishRoute.PositionDataList.Add((new PositionData(captureLat,
                                captureLon)));

                            RouteChooser routeChooser = new RouteChooser(captureLat, captureLon, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);

                            while (!chosenPosition)
                            {
                                randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                            }

                            while (!addedToPosDataList)
                            {
                                addedToPosDataList = fishRoute.PositionDataList.TryAdd((new PositionData(
                                    validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                    validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp, FishList["742"].TagDataList[i].depth,
                                    FishList["742"].TagDataList[i].temp, validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));
                            }

                            while (!addedToFishRouteList)
                            {
                                addedToFishRouteList = FishList["742"].FishRouteList.TryAdd(fishRoute);
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
                                    possiblePositionsArray = CalculateCoordinates.CalculatePossibleEtaXi(pData.eta_rho, pData.xi_rho, Math.Abs(pData.depth - tagData.depth) < 30);
                                    validPositionsDataList =
                                        CalculateCoordinates.FindValidPositions(
                                            possiblePositionsArray,
                                            HeatMap.LatArray, HeatMap.LonArray, tagData, TempContainer, TempDelta);
                                }



                                if (validPositionsDataList.Count > 0)
                                {
                                    RouteChooser routeChooser =
                                            new RouteChooser(pData.lat, pData.lon, FishList["742"].ReleaseLat, FishList["742"].ReleaseLon);
                                    while (!chosenPosition)
                                    {
                                        randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                        chosenPosition = routeChooser.ChosenRoute(validPositionsDataList, randInt);
                                    }
                                    fishRoute.PositionDataList.Add((new PositionData(
                                            validPositionsDataList.ElementAt(randInt).lat,
                                            validPositionsDataList.ElementAt(randInt).lon,
                                            validPositionsDataList.ElementAt(randInt).depth,
                                            validPositionsDataList.ElementAt(randInt).temp,
                                            tagData.depth, tagData.temp,
                                            validPositionsDataList.ElementAt(randInt).eta_rho,
                                            validPositionsDataList.ElementAt(randInt).xi_rho)));

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
                        i = halfTagDataCount;
                    }

                    dayCounter++;
                    counter++;
                }

                if (dayCounter == 2 && DayIncrement < 1)
                {
                    dayCounter = 0;
                    day--;
                }
                else if (DayIncrement >= 1)
                {
                    day -= (int)DayIncrement;
                }
            }

            watch.Stop();
            double elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Hvor lang tid tok programmet: " + elapsedMs);
            var count = 1;
            var FishData = FishList["742"];

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
                DirectoryInfo di = new DirectoryInfo(GlobalVariables.pathToSaveFishData + @"\BW\");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            Console.WriteLine("Day is: {0}", day);

            foreach (var fishRoute in FishList["742"].FishRouteList)
            {
                if (fishRoute.Alive)
                {
                    string[] fishData = fishRoute.FromListToString();

                    System.IO.File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\BW\" + fishRoute.Id + "_" + count + ".txt", fishData);
                    count++;
                }
            }
        }
    }
}