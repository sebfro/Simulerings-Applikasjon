using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpagettiMetoden
{
    class Program
    {
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


            //Dette er f�rste koordinat til alle fisker som blir sluppet ut i tilfeldige retninger fra denne koordinaten
            //Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);


            //Regne ut posisjoner fisken kan dra til fra release lat lon basert p� den f�rste m�lingen i merkedata
            //Lagre disse i en array

            //Returnerer 360 lat lon verdier, 1 for hver vinkel


            /*
             * LatLon[] latLons =
                calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);
             */

            Random random = new Random();
            int randInt = 0;
           
            //Array tempArray = ds["temp"].GetData();
            int counter = 0;

            HeatMap heatMap = new HeatMap("2003","08");

            for (int i = 0; i < FishList["742"].tagDataList.Count; i+=1000)
            {
                Console.WriteLine("I iterasjon: " + i / 1000);
                bool chosenPosition = false;
                double randDouble = 0.0;
                double originalPosition = 0.0;
                double newPosition = 0.0;
                int chosenPositionCounter = 0;
                if (FishList["742"].tagDataList[i].month != heatMap.month ||
                    FishList["742"].tagDataList[i].year != heatMap.year)
                {
                    Console.WriteLine("Fishlist month: " + FishList["742"].tagDataList[i].month + "heatMap month: " + heatMap.month + " Fishlist year: " + FishList["742"].tagDataList[i].year + " heatMap year: " + heatMap.year);
                    heatMap = new HeatMap(FishList["742"].tagDataList[i].year, FishList["742"].tagDataList[i].month);
                }

                if (i == 0)
                {
                    LatLon[] latLons =
                        calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);

                    List<PositionData> validPositionsDataList =
                        calcDistanceBetweenTwoLonLatCoordinates.FindValidLatLons(latLons, heatMap.latArray, heatMap.lonArray, FishList["742"].tagDataList[i], heatMap.depthArray, Z_Array);

                    for (int j = 0; j < GlobalVariables.releasedFish; j++)
                    {
                        //Console.WriteLine("Fisk nr: " + j + " , i iterasjon: " + i / 1000);
                        if (validPositionsDataList.Count > 0)
                        {
                            FishList["742"].FishRouteList.Add(new FishRoute("742"));
                            FishList["742"].FishRouteList[j].PositionDataList.Add((new PositionData(FishList["742"].releaseLat,
                                FishList["742"].releaseLon, 0.0, 0.0, 0.0 , 0.0)));


                            originalPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(FishList["742"].releaseLat,
                                FishList["742"].releaseLon, FishList["742"].captureLat, FishList["742"].captureLon);

                            chosenPositionCounter = 0;
                            while(!chosenPosition && (chosenPositionCounter < 2)) {
                                randDouble = random.NextDouble();
                                randInt = random.Next(validPositionsDataList.Count);
                                newPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(validPositionsDataList[randInt].lat,
                                    validPositionsDataList[randInt].lon, FishList["742"].captureLat, FishList["742"].captureLon);
                                
                                
                                if(newPosition <= originalPosition && randDouble <= 0.7) {
                                    chosenPosition = true;
                                } else if(newPosition >= originalPosition && randDouble >= 0.7) {
                                    chosenPosition = true;
                                }
                                chosenPositionCounter++;
                            }

                            FishList["742"].FishRouteList[j].PositionDataList.Add((new PositionData(
                                validPositionsDataList[randInt].lat, validPositionsDataList[randInt].lon,
                                validPositionsDataList[randInt].depth, validPositionsDataList[randInt].temp, FishList["742"].tagDataList[i].depth, FishList["742"].tagDataList[i].temp)));
                        }
                        else
                        {
                            Console.WriteLine("No possible positions found");
                            return;
                        }
                    }


                    foreach (var p in validPositionsDataList)
                    {
                        //Console.WriteLine("Lat: " + p.lat + ", lon: " + p.lon);
                       // Console.WriteLine("eta: " + p.eta_rho + ", xi: " + p.xi_rho);
                    }
                   // System.Console.ReadLine();

                    counter = 2;
                }
                else
                {
                    List<FishRoute> fishRoutes = FishList["742"].FishRouteList;
                    TagData tagData = FishList["742"].tagDataList[i];
                    for (int j = 0; j < GlobalVariables.releasedFish; j++)
                    {
                        //Console.WriteLine("Fisk nr: " + j + ", i iterasjon: " + i / 1000);
                        FishRoute fishRoute = fishRoutes[j];

                            if(fishRoute.alive)
                            {
                                PositionData pData = fishRoute.PositionDataList[counter - 1];
                                LatLon[] latLons =
                                    calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(pData.lat, pData.lon, 10, 167);
                                List<PositionData> validPositionsDataList =
                                    calcDistanceBetweenTwoLonLatCoordinates.FindValidLatLons(latLons, heatMap.latArray, heatMap.lonArray, tagData, heatMap.depthArray, Z_Array);

                                if (validPositionsDataList.Count > 0)
                                {
                                    originalPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(pData.lat,
                                        pData.lon, FishList["742"].captureLat, FishList["742"].captureLon);

                                chosenPositionCounter = 0;
                                    while (!chosenPosition && (chosenPositionCounter < 2))
                                    {
                                    randDouble = random.NextDouble();
                                        randInt = random.Next(validPositionsDataList.Count);
                                        newPosition = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(validPositionsDataList[randInt].lat,
                                            validPositionsDataList[randInt].lon, FishList["742"].captureLat, FishList["742"].captureLon);
                                
                                        if(newPosition <= originalPosition && randDouble <= 0.7) {
                                            chosenPosition = true;
                                        } else if(newPosition >= originalPosition && randDouble >= 0.7) {
                                            chosenPosition = true;
                                        }
                                    }



                                    randInt = random.Next(validPositionsDataList.Count);
                                    fishRoutes[j].PositionDataList.Add((new PositionData(validPositionsDataList[randInt].lat, validPositionsDataList[randInt].lon, validPositionsDataList[randInt].depth, validPositionsDataList[randInt].temp, tagData.depth, tagData.temp)));
                                }
                                else
                                {
                                    fishRoute.commitNotAlive();
                                Console.WriteLine("Fisk nr: " + j + ", i iterasjon: " + i / 1000 + " ELIMINERT");
                                Console.WriteLine("dybde: " + tagData.depth + ", temp: " + tagData.temp);
                                }
                            }

                    }
                    
                    counter++;
                }
                
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
                calcDistanceBetweenTwoLonLatCoordinates.FindValidLatLons(latLons, latArray, lonArray);
             */




            //  foreach antall fisker som blir sendt ut
            //  Velg en random posisjon
            //  if randomposisjon.distanse > startposisjon.distanse fra capture.position
            //  30% sannsynlighet for � velge denne
            //  else
            //  70% sannsynlighet  for � velge denne
            //  if ikke velger denne posisjonen
            //  Velg ny random
            //        
            //  Lagre lat lon, temp for fisk og dybde for fisk --> Lagre temp og dybde slik at det blir lettere � modellere i 2D og 3D
            //  foreach tagData i tagDataList
            //  Hent ut n�v�rende posisjon
            //  Regne ut posisjoner fisken kan dra til fra current lat lon basert p� neste m�ling i merkedata
            //  Lagre disse i en array
            //  if array er tom ELIMINER
            // else
            //if randomposisjon.distanse > n�v�rende.distanse fra capture.position
            //  30% sannsynlighet for � velge denne
            //  else
            //  70% sannsynlighet  for � velge denne
            //  if ikke velger denne posisjonen
            //  Velg ny random
            //
            //  Lagre lat lon, temp for fisk og dybde for fisk --> Lagre temp og dybde slik at det blir lettere � modellere i 2D og 3D
            //  Kj�r foreach p� nytt
            //  Slutt p� foreach


            //Bruke release lat lon til � finne eta_rho og xi_rho




            //Kalkulerer for en fisk for �yeblikket for release lat og lon

            /*
             * positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, ds["h"].GetData());
            Console.WriteLine("depth: " + positionData.depth);

            DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, positionData.depth, Z_Array);

            positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, ds["temp"].GetData());
            Console.WriteLine("temp: " + positionData.temp);
             */

            //FishList["742"].PositionDataList[0].Add(positionData);

            //Slutten p� foreach

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
