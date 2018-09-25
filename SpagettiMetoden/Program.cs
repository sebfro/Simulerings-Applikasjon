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
        private readonly object randomLock = new object();
        private static readonly ThreadLocal<Random> ThreadSafeRandom
            = new ThreadLocal<Random>(() => new Random());

        static void Main(string[] args)
        {
            ReadFromFile file = new ReadFromFile();

            Dictionary<string, Fish> FishList = new Dictionary<string, Fish>();
            List<string> KeyList = new List<string>();

            //DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\mndmean_avg_200309.nc");
            DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\ocean_avg_20030801.nc");

            DataSet dsOfZ = DataSet.Open(@"C:\NCdata\VarmeModell\NS4MI_Z.nc");
            Array Z_Array = dsOfZ["Z"].GetData();

            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            CalcDistance_BetweenTwoLonLatCoordinates calcDistanceBetweenTwoLonLatCoordinates = new CalcDistance_BetweenTwoLonLatCoordinates();
            

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);

            




        //Dette er første koordinat til alle fisker som blir sluppet ut i tilfeldige retninger fra denne koordinaten
        //Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);


        //Regne ut posisjoner fisken kan dra til fra release lat lon basert pø den første målingen i merkedata
        //Lagre disse i en array

        //Returnerer 360 lat lon verdier, 1 for hver vinkel


        /*
         * LatLon[] latLons =
            calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);
         */

        Random random = new Random();
            int randInt = 0;
           
            int counter = 0;

            HeatMap heatMap = new HeatMap();
            EtaXi[] etaXis = new EtaXi[0];
            for (int i = 0; i < FishList["742"].tagDataList.Count; i+=1000)
            {
                var watch = Stopwatch.StartNew();
                Console.WriteLine("I iterasjon: " + i / 1000);
                bool chosenPosition;
                double randDouble = 0.0;
                double originalPosition = 0.0;
                double newPosition = 0.0;
                int chosenPositionCounter = 0;
                //heatMap month og year er ikke inistialisert, sjekk om det ødelegger
                if (FishList["742"].tagDataList[i].month != heatMap.month ||
                    FishList["742"].tagDataList[i].year != heatMap.year || !heatMap.initialized)
                {
                    Console.WriteLine("Fishlist month: " + FishList["742"].tagDataList[i].month + "heatMap month: " + heatMap.month + " Fishlist year: " + FishList["742"].tagDataList[i].year + " heatMap year: " + heatMap.year);
                    heatMap = new HeatMap(FishList["742"].tagDataList[i].year, FishList["742"].tagDataList[i].month);
                }

                if (i == 0)
                {
                    PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(heatMap.latArray, heatMap.lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);
                    etaXis = calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleEtaXi(positionData.eta_rho, positionData.xi_rho);

                    //LatLon[] latLons =
                    //calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);

                    BlockingCollection<PositionData> validPositionsDataList =
                        calcDistanceBetweenTwoLonLatCoordinates.FindValidPositions(etaXis, heatMap.latArray, heatMap.lonArray, FishList["742"].tagDataList[i], heatMap.depthArray, Z_Array);

                    for (int j = 0; j < GlobalVariables.releasedFish; j++)
                    {
                        chosenPosition = false;
                        //Console.WriteLine("Fisk nr: " + j + " , i iterasjon: " + i / 1000);
                        if (validPositionsDataList.Count > 0)
                        {
                            FishList["742"].FishRouteList.Add(new FishRoute("742"));
                            FishList["742"].FishRouteList.ElementAt(j).PositionDataList.Add((new PositionData(FishList["742"].releaseLat,
                                FishList["742"].releaseLon)));


                            originalPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(FishList["742"].releaseLat,
                                FishList["742"].releaseLon, FishList["742"].captureLat, FishList["742"].captureLon);

                            chosenPositionCounter = 0;
                            while(!chosenPosition) {
                                randDouble = random.NextDouble();
                                randInt = random.Next(validPositionsDataList.Count);
                                newPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(validPositionsDataList.ElementAt(randInt).lat,
                                    validPositionsDataList.ElementAt(randInt).lon, FishList["742"].captureLat, FishList["742"].captureLon);
                                
                                
                                if(newPosition <= originalPosition && randDouble <= 0.7) {
                                    chosenPosition = true;
                                } else if(newPosition >= originalPosition && randDouble >= 0.7) {
                                    chosenPosition = true;
                                }
                                chosenPositionCounter++;
                            }

                            FishList["742"].FishRouteList.ElementAt(j).PositionDataList.Add((new PositionData(
                                validPositionsDataList.ElementAt(randInt).lat, validPositionsDataList.ElementAt(randInt).lon,
                                validPositionsDataList.ElementAt(randInt).depth, validPositionsDataList.ElementAt(randInt).temp, FishList["742"].tagDataList[i].depth,
                                FishList["742"].tagDataList[i].temp, validPositionsDataList.ElementAt(randInt).eta_rho, validPositionsDataList.ElementAt(randInt).xi_rho)));
                        }
                        else
                        {
                            Console.WriteLine("No possible positions found");
                            return;
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
                        lock (new Program().randomLock)
                        {
                            

                        chosenPosition = false;


                        //Console.WriteLine("Fisk nr: " + j + ", i iterasjon: " + i / 1000);
                        //FishRoute fishRoute = fishRoutes[j];

                        if (fishRoute.alive)
                        {
                            PositionData pData = fishRoute.PositionDataList[counter - 1];
                            //LatLon[] latLons =
                            //    calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(pData.lat, pData.lon, 10, 167);

                            etaXis = calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleEtaXi(pData.eta_rho,
                                pData.xi_rho);
                            BlockingCollection<PositionData> validPositionsDataList =
                                calcDistanceBetweenTwoLonLatCoordinates.FindValidPositions(etaXis, heatMap.latArray,
                                    heatMap.lonArray, tagData, heatMap.depthArray, Z_Array);

                            if (validPositionsDataList.Count > 0)
                            {
                                originalPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(
                                    pData.lat,
                                    pData.lon, FishList["742"].captureLat, FishList["742"].captureLon);
                                
                                while (!chosenPosition)
                                {
                                    randDouble = ThreadSafeRandom.Value.NextDouble();
                                    randInt = ThreadSafeRandom.Value.Next(0, validPositionsDataList.Count);
                                    newPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(
                                        validPositionsDataList.ElementAt(randInt).lat,
                                        validPositionsDataList.ElementAt(randInt).lon, FishList["742"].captureLat,
                                        FishList["742"].captureLon);

                                    if (newPosition <= originalPosition && randDouble <= 0.7)
                                    {
                                        chosenPosition = true;
                                    }
                                    else if (newPosition >= originalPosition && randDouble >= 0.7)
                                    {
                                        chosenPosition = true;
                                    }
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
                                Console.WriteLine("I iterasjon: " + i / 1000 + " ELIMINERT");
                                Console.WriteLine("eta: " + pData.eta_rho + ", xi: " + pData.xi_rho);
                                Console.WriteLine("dybde: " + tagData.depth + ", temp: " + tagData.temp);
                                Console.WriteLine("dybde: " + pData.depth + ", temp: " + pData.temp);
                            }
                        }

                        }
                    });
                    
                    
                    counter++;
                }
                watch.Stop();
                double elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Hvor lang tid tok interasjon " + i / 1000 + ": " + elapsedMs);

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
            
            /*
             * List<PositionData> positionDataList =
                calcDistanceBetweenTwoLonLatCoordinates.FindValidPositions(latLons, latArray, lonArray);
             */




            //  foreach antall fisker som blir sendt ut
            //  Velg en random posisjon
            //  if randomposisjon.distanse > startposisjon.distanse fra capture.position
            //  30% sannsynlighet for å velge denne
            //  else
            //  70% sannsynlighet  for å velge denne
            //  if ikke velger denne posisjonen
            //  Velg ny random
            //        
            //  Lagre lat lon, temp for fisk og dybde for fisk --> Lagre temp og dybde slik at det blir lettere å modellere i 2D og 3D
            //  foreach tagData i tagDataList
            //  Hent ut nåvårende posisjon
            //  Regne ut posisjoner fisken kan dra til fra current lat lon basert på neste måling i merkedata
            //  Lagre disse i en array
            //  if array er tom ELIMINER
            // else
            //if randomposisjon.distanse > nåvårende.distanse fra capture.position
            //  30% sannsynlighet for å velge denne
            //  else
            //  70% sannsynlighet  for å velge denne
            //  if ikke velger denne posisjonen
            //  Velg ny random
            //
            //  Lagre lat lon, temp for fisk og dybde for fisk --> Lagre temp og dybde slik at det blir lettere å modellere i 2D og 3D
            //  Kjår foreach på nytt
            //  Slutt på foreach


            //Bruke release lat lon til å finne eta_rho og xi_rho




            //Kalkulerer for en fisk for åyeblikket for release lat og lon

            /*
             * positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, ds["h"].GetData());
            Console.WriteLine("depth: " + positionData.depth);

            DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, positionData.depth, Z_Array);

            positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, ds["temp"].GetData());
            Console.WriteLine("temp: " + positionData.temp);
             */

            //FishList["742"].PositionDataList[0].Add(positionData);

            //Slutten på foreach

            /*
            Console.WriteLine(FishList["742"].PositionDataList[0].lat);
            Console.WriteLine(FishList["742"].PositionDataList[0].lon);
            Console.WriteLine(FishList["742"].PositionDataList[0].eta_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].xi_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].depth);
            Console.WriteLine(FishList["742"].PositionDataList[0].temp);
            */

            /*
             foreach (TagData tagD in FishList["742"].tagDataList)
            {
                Console.WriteLine("Depth: " + tagD.depth + ", Temp: " + tagD.temp + ", Date: " + tagD.date + ", Time: " + tagD.time);
            }
             */


            /*
            double distance =
                calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(49.1715000, -121.7493500,
                    latLons[0].lat, latLons[0].lon);

            Console.WriteLine(distance);

            int counter = 0;
            foreach (LatLon latLon in latLons)
            {
                counter++;
                Console.WriteLine(calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(49.1715000, -121.7493500,
                    latLon.lat, latLon.lon));
            }
            Console.WriteLine(counter);
            */
            Console.ReadLine();
        }
    }
}