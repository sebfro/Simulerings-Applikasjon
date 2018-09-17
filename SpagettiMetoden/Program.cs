using Microsoft.Research.Science.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Science.Data.Imperative;

namespace SpagettiMetoden
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadFromFile file = new ReadFromFile();

            Dictionary<string, Fish> FishList = new Dictionary<string, Fish>();
            List<string> KeyList = new List<string>();

            DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\mndmean_avg_200309.nc");
            //DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\ocean_avg_20030801.nc");

            /*
             * string temp = ds.URI.Split('_','.')[2];
            int startIndex = 4;
            int endIndex = temp.Length - 6;
            int month = int.Parse(temp.Substring(startIndex, endIndex));
             */
            DataSet dsOfZ = DataSet.Open(@"C:\NCdata\VarmeModell\NS4MI_Z.nc");

            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            CalcDistance_BetweenTwoLonLatCoordinates calcDistanceBetweenTwoLonLatCoordinates = new CalcDistance_BetweenTwoLonLatCoordinates();

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);
            

            string[] dateStrings = FishList["742"].tagDataList[0].date.Split('-');



            var latArray = ds["lat_rho"].GetData();
            var lonArray = ds["lon_rho"].GetData();
            var ocean_time = ds["ocean_time"].GetData();
            var Z_Array = dsOfZ["Z"].GetData();


            //Dette er første koordinat til alle fisker som blir sluppet ut i tilfeldige retninger fra denne koordinaten
            Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);


            //Regne ut posisjoner fisken kan dra til fra release lat lon basert på den første målingen i merkedata
            //Lagre disse i en array

            //Returnerer 360 lat lon verdier, 1 for hver vinkel


            /*
             * LatLon[] latLons =
                calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);
             */

            PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(latArray, lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);


            positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, ds["h"].GetData());
            Console.WriteLine("depth: " + positionData.depth);

            DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, positionData.depth, Z_Array);

            positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, ds["temp"].GetData());
            Console.WriteLine("temp: " + positionData.temp);

            for (int i = 0; i < FishList["742"].tagDataList.Count; i++)
            {
                //Må hente riktig varmemodell
                Array depthArray = ds["h"].GetData();
                Array tempArray = ds["temp"].GetData();
                if (i == 0)
                {
                    LatLon[] latLons =
                        calcDistanceBetweenTwoLonLatCoordinates.calculatePossibleLatLon(FishList["742"].releaseLat, FishList["742"].releaseLon, 20, 1);

                    List<PositionData> positionDataList =
                        calcDistanceBetweenTwoLonLatCoordinates.FindValidLatLons(latLons, latArray, lonArray, FishList["742"].tagDataList[i], depthArray, tempArray, Z_Array);
                    foreach (var p in positionDataList)
                    {
                        Console.WriteLine("Lat: " + p.lat + ", lon: " + p.lon);
                    }
                }
                else
                {
                }
            }
            /*
             * List<PositionData> positionDataList =
                calcDistanceBetweenTwoLonLatCoordinates.FindValidLatLons(latLons, latArray, lonArray);
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
            //  Hent ut nåværende posisjon
            //  Regne ut posisjoner fisken kan dra til fra current lat lon basert på neste måling i merkedata
            //  Lagre disse i en array
            //  if array er tom ELIMINER
            // else
            //if randomposisjon.distanse > nåværende.distanse fra capture.position
            //  30% sannsynlighet for å velge denne
            //  else
            //  70% sannsynlighet  for å velge denne
            //  if ikke velger denne posisjonen
            //  Velg ny random
            //
            //  Lagre lat lon, temp for fisk og dybde for fisk --> Lagre temp og dybde slik at det blir lettere å modellere i 2D og 3D
            //  Kjør foreach på nytt
            //  Slutt på foreach


            //Bruke release lat lon til å finne eta_rho og xi_rho




            //Kalkulerer for en fisk for øyeblikket for release lat og lon

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
            System.Console.ReadLine();
        }
    }
}
