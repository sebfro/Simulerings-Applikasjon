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
            DataSet dsOfZ = DataSet.Open(@"C:\NCdata\VarmeModell\NS4MI_Z.nc");

            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);

            var latArray = ds["lat_rho"].GetData();
            var lonArray = ds["lon_rho"].GetData();
            var ocean_time = ds["ocean_time"].GetData();
            var Z_Array = dsOfZ["Z"].GetData();


            //Dette er første koordinat til alle fisker som blir sluppet ut i tilfeldige retninger fra denne koordinaten
            Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);

            //Regne ut posisjoner fisken kan dra til fra release lat lon basert på den første målingen i merkedata
            //Lagre disse i en array

            
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
            PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(latArray, lonArray, FishList["742"]);




            //Kalkulerer for en fisk for øyeblikket for release lat og lon
            
            positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, ds["h"].GetData());
            Console.WriteLine("depth: " + positionData.depth);

            DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, positionData.depth, Z_Array);

            positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, ds["temp"].GetData());
            Console.WriteLine("temp: " + positionData.temp);

            FishList["742"].PositionDataList[0].Add(positionData);

            //Slutten på foreach

            Console.WriteLine(FishList["742"].PositionDataList[0].lat);
            Console.WriteLine(FishList["742"].PositionDataList[0].lon);
            Console.WriteLine(FishList["742"].PositionDataList[0].eta_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].xi_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].depth);
            Console.WriteLine(FishList["742"].PositionDataList[0].temp);

            /*
             foreach (TagData tagD in FishList["742"].tagDataList)
            {
                Console.WriteLine("Depth: " + tagD.depth + ", Temp: " + tagD.temp + ", Date: " + tagD.date + ", Time: " + tagD.time);
            }
             */

            CalcDistance_BetweenTwoLonLatCoordinates calcDistanceBetweenTwoLonLatCoordinates = new CalcDistance_BetweenTwoLonLatCoordinates();

            double res = calcDistanceBetweenTwoLonLatCoordinates.getDistanceFromLatLonInKm(49.1715000, -121.7493500, 49.18258,
                -121.75441);

            Console.WriteLine("Distance in km: " + res);


            DataSet dsTemp = DataSet.Open(@"C:\NCdata\VarmeModell\ocean_avg_20030801.nc");
            var oceanTime = dsTemp["ocean_time"].GetData();

            foreach (double ocTime in oceanTime)
            {
                Console.WriteLine("Oceantime: " + ocTime);
            }



            System.Console.ReadLine();
        }
    }
}
