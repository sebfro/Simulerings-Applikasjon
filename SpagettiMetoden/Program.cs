using Microsoft.Research.Science.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


            
            Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);

            //Bruke release lat lon til å finne eta_rho og xi_rho
            PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(latArray, lonArray, FishList["742"]);




            //Kalkulerer for en fisk for øyeblikket for release lat og lon
            
            positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, ds["h"].GetData());
            Console.WriteLine("depth: " + positionData.depth);

            DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, positionData.depth, Z_Array);

            positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, ds["temp"].GetData());
            Console.WriteLine("temp: " + positionData.temp);

            FishList["742"].PositionDataList.Add(positionData);

            //Slutten på foreach

            Console.WriteLine(FishList["742"].PositionDataList[0].lat);
            Console.WriteLine(FishList["742"].PositionDataList[0].lon);
            Console.WriteLine(FishList["742"].PositionDataList[0].eta_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].xi_rho);
            Console.WriteLine(FishList["742"].PositionDataList[0].depth);
            Console.WriteLine(FishList["742"].PositionDataList[0].temp);
           


            System.Console.ReadLine();
        }
    }
}
