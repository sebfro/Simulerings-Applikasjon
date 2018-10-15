using Microsoft.Research.Science.Data;
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
    class Controller
    {
        public ReadFromFile file { get; set; }
        Dictionary<string, Fish> FishList { get; set; }
        List<string> KeyList { get; set; }

        HeatMap HeatMap { get; set; }
        EtaXi[] EtaXis { get; set; }
        public CallPython callPython { get; set; }
        public CalcDistance_BetweenTwoLonLatCoordinates calcDistance_BetweenTwoLonLatCoordinates { get; set; }

        public int TagStep { get; set; }
        public int DayIncrement { get; set; }
        public int ReleasedFish { get; set; }
        public double TempDelta { get; set; }

        public void SetDayIncrement(int dayInc)
        {
            DayIncrement = dayInc;
            if(dayInc == 4)
            {
                TagStep = 580;
            } else if( dayInc == 3)
            {
                TagStep = 435;
            } else if( dayInc == 2)
            {
                TagStep = 290;
            } else
            {
                TagStep = 145;
            }
        }

        public Controller(int dayInc, int releasedFish, double tempDelta, int depthDelta, int Increment, int Increment2)
        {
            TempDelta = tempDelta;
            ReleasedFish = releasedFish;
            SetDayIncrement(dayInc);

            file = new ReadFromFile();

            FishList = new Dictionary<string, Fish>();
            KeyList = new List<string>();

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);



            HeatMap = new HeatMap();
            EtaXis = new EtaXi[0];
            callPython = new CallPython(dayInc);
            calcDistance_BetweenTwoLonLatCoordinates = new CalcDistance_BetweenTwoLonLatCoordinates(Increment, Increment2, depthDelta);
        }

        public void SetIncrements(int Increment, int Increment2)
        {
            calcDistance_BetweenTwoLonLatCoordinates.Increment = Increment;
            calcDistance_BetweenTwoLonLatCoordinates.Increment2 = Increment2;
        }

        public void SetDepthDelta(int DepthDelta)
        {
            calcDistance_BetweenTwoLonLatCoordinates.SetDepthDelta(DepthDelta);
        }

        public void RunAlgorithm()
        {
            int day = GlobalVariables.day;
            int counter = 1;
            int deadFishCounter = 0;
            var watch = Stopwatch.StartNew();
            FishList["742"].FishRouteList = new BlockingCollection<FishRoute>();

            for (int i = 0; i < FishList["742"].tagDataList.Count; i += TagStep)
            {
                Console.WriteLine("I iterasjon: " + i / TagStep);
                bool chosenPosition;

                if (i == 0)
                {
                    var watch2 = Stopwatch.StartNew();

                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(HeatMap.LatArray, HeatMap.LonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);
                    BlockingCollection<PositionData> validPositionsDataList =
                        calcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(calcDistance_BetweenTwoLonLatCoordinates.CalculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho),
                        HeatMap.LatArray, HeatMap.LonArray, FishList["742"].tagDataList[i], day, callPython, TempDelta);

                    float releaseLat = (float)FishList["742"].releaseLat;
                    float releaseLon = (float)FishList["742"].releaseLon;

                    Parallel.For(0, ReleasedFish, (j) =>
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
                    if (deadFishCounter < ReleasedFish)
                    {
                        Parallel.ForEach(fishRoutes, (fishRoute) =>
                        {
                            int randInt = 0;
                            chosenPosition = false;

                            if (fishRoute.alive)
                            {
                                PositionData pData = fishRoute.PositionDataList.ElementAt(counter);

                                BlockingCollection<PositionData> validPositionsDataList =
                                    calcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(calcDistance_BetweenTwoLonLatCoordinates.CalculatePossibleEtaXi(pData.eta_rho,
                                    pData.xi_rho), HeatMap.LatArray, HeatMap.LonArray, tagData, day, callPython, TempDelta);

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
                                    */
                                }
                            }
                        });
                    }
                    else
                    {
                        i = FishList["742"].tagDataList.Count;
                    }

                    counter++;
                }
                day += DayIncrement;


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
                    var posData = fishRoute.PositionDataList.ElementAt(fishRoute.PositionDataList.Count - 1);
                    if (CalcDistance_BetweenTwoLonLatCoordinates.GetDistanceFromLatLonInKm(posData.lat, posData.lon, captureLat, captureLon) < calcDistance_BetweenTwoLonLatCoordinates.Increment)
                    {
                        folderName = "Akseptabel";
                    } else
                    {
                        folderName = "Uakseptabel";
                    }
                    string[] fishData = fishRoute.fromListToString();

                    File.WriteAllLines(GlobalVariables.pathToSaveFishData + @"\\" + folderName + "\\" + fishRoute.id + "_" + count + ".txt", fishData);
                    count++;
                }
            }
            Console.WriteLine("Hvor lang tid tok programmet: {0} minutter.", elapsedMs / 60000);
            Console.WriteLine("Hvor lang tid tok programmet: {0} sekunder.", elapsedMs / 1000);
            Console.WriteLine("Dead fish counter: {0}", deadFishCounter);
            Console.WriteLine("Alive fish counter: {0}", ReleasedFish - deadFishCounter);
            if (deadFishCounter == ReleasedFish)
            {
                Console.WriteLine("All fish are dead");
            }
            Console.WriteLine("Day is: {0}", day);
        }
    }
}
