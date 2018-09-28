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
            ReadFromFile file = new ReadFromFile();

            Dictionary<string, Fish> FishList = new Dictionary<string, Fish>();
            List<string> KeyList = new List<string>();

            //DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMaps + "20030801.nc");
            /*
            DataSet ds = DataSet.Open(@"I:\VarmeModell\norkyst_800m_avg.nc");

            var latArray = ds["lat_rho"].GetData();
            var lonArray = ds["lon_rho"].GetData();
            var temp = ds["temp"].GetData();
            var vTransformArray = ds["Vtransform"].GetData();
            Console.WriteLine(vTransformArray.GetValue(0));
            var vStretchingArray = ds["Vstretching"].GetData();
            var theta_s = ds["theta_s"].GetData();
            var theta_b = ds["theta_b"].GetData();
            var Tcline = ds["Tcline"].GetData();

            

            Console.WriteLine("theta_s: {0}, theta_b: {1}, Tcline: {2}", theta_s.GetValue(0), theta_b.GetValue(0), Tcline.GetValue(0));
            foreach(var v in temp)
            {
                Console.WriteLine(v);
            }

            Console.WriteLine("Success");
            Console.ReadLine();
            */
            DataSet dsOfZ = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NK800_Z.nc");
            Array Z_Array = dsOfZ["Z"].GetData();

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);
           
            int counter = 0;

            HeatMap heatMap = new HeatMap();
            EtaXi[] etaXis = new EtaXi[0];
            int day = 27;
            int tagStep = 500;

            for (int i = 0; i < FishList["742"].tagDataList.Count; i+=tagStep)
            {
                
                var watch = Stopwatch.StartNew();
                Console.WriteLine("I iterasjon: " + i / tagStep);
                bool chosenPosition;

                /*
                //heatMap month og year er ikke inistialisert, sjekk om det ødelegger
                if (FishList["742"].tagDataList[i].month != heatMap.month ||
                    FishList["742"].tagDataList[i].year != heatMap.year || !heatMap.initialized)
                {
                    Console.WriteLine("Fishlist month: " + FishList["742"].tagDataList[i].month + "heatMap month: " + heatMap.month + " Fishlist year: " + FishList["742"].tagDataList[i].year + " heatMap year: " + heatMap.year);
                    heatMap = new HeatMap(FishList["742"].tagDataList[i].year, FishList["742"].tagDataList[i].month);
                }
                */

                if (i == 0)
                {
                    int randInt = 0;
                    PositionData positionData = CalculateXiAndEta.GeneratePositionDataArrayList(heatMap.latArray, heatMap.lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);
                    
                    BlockingCollection<PositionData> validPositionsDataList =
                        CalcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(CalcDistance_BetweenTwoLonLatCoordinates.calculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho), 
                        heatMap.latArray, heatMap.lonArray, FishList["742"].tagDataList[i], heatMap.depthArray, Z_Array, day);

                    for (int j = 0; j < GlobalVariables.releasedFish; j++)
                    {
                        chosenPosition = false;

                        if (validPositionsDataList.Count > 0)
                        {
                            FishList["742"].FishRouteList.Add(new FishRoute("742"));
                            FishList["742"].FishRouteList.ElementAt(j).PositionDataList.Add((new PositionData(FishList["742"].releaseLat,
                                FishList["742"].releaseLon)));

                                RouteChooser routeChooser = new RouteChooser(FishList["742"].releaseLat, FishList["742"].releaseLon, FishList["742"]);

                                while (!chosenPosition)
                                {
                                    randInt = ThreadSafeRandom.Next(validPositionsDataList.Count);
                                    chosenPosition = routeChooser.chosenRoute(validPositionsDataList, randInt);
                                }

                        FishList["742"].FishRouteList.ElementAt(j).PositionDataList.Add((new PositionData(
                                validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp, FishList["742"].tagDataList[i].depth,
                                FishList["742"].tagDataList[i].temp, validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));

                            //Console.WriteLine("New Position. Lat: {0}, lon: {1}", validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon);

                        }
                        else
                        {
                            Console.WriteLine("No possible positions found");
                        }
                    }
                    counter = 2;
                }
                else
                {
                    BlockingCollection<FishRoute> fishRoutes = FishList["742"].FishRouteList;
                    TagData tagData = FishList["742"].tagDataList[i];

                    Parallel.ForEach(fishRoutes, (fishRoute) =>
                    {
                        int randInt = 0;
                        chosenPosition = false;

                        if (fishRoute.alive)
                        {
                            PositionData pData = fishRoute.PositionDataList.ElementAt(counter - 1);

                            BlockingCollection<PositionData> validPositionsDataList =
                                CalcDistance_BetweenTwoLonLatCoordinates.FindValidPositions(CalcDistance_BetweenTwoLonLatCoordinates.calculatePossibleEtaXi(pData.eta_rho,
                                pData.xi_rho), heatMap.latArray, heatMap.lonArray, tagData, heatMap.depthArray, Z_Array, day);

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
                                fishRoute.commitNotAlive();
                                Console.WriteLine("I iterasjon: " + i / tagStep + " ELIMINERT");
                                Console.WriteLine("eta: " + pData.eta_rho + ", xi: " + pData.xi_rho);
                                Console.WriteLine("dybde: " + tagData.depth + ", temp: " + tagData.temp);
                                Console.WriteLine("dybde: " + pData.depth + ", temp: " + pData.temp);
                            }
                        }
                    });
                    
                    
                    counter++;
                }
                day += 3;
                watch.Stop();
                double elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Hvor lang tid tok interasjon " + i / tagStep + ": " + elapsedMs);

            }
            var count = 1;
            foreach (var fishRoute in FishList["742"].FishRouteList)
            {
                Console.WriteLine("Is fish alive?: " + fishRoute.alive);
                
                if(fishRoute.alive)
                {
                    string[] fishData = fishRoute.fromListToString();

                    File.WriteAllLines(@"C:\NCdata\fishData\fishData" + fishRoute.id +"_" + count + ".txt", fishData);
                    count++;
                }
            }
            Console.ReadLine();
        }
    }
}  
   