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

            file.readReleaseAndCapture(FishList, KeyList);
            file.readTagData(FishList, KeyList);

            

            DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\mndmean_avg_200309.nc");

            var latArray = ds["lat_rho"].GetData();
            var lonArray = ds["lon_rho"].GetData();

            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();

            Console.WriteLine("Lat: " + FishList["742"].releaseLat + ", Lon: " + FishList["742"].releaseLon);
            ArrayList potentialPositionsArrayList = calculateXiAndEta.GeneratePositionDataArrayList(latArray, lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);

            int[] etaAndXi = calculateXiAndEta.ConvertLatAndLonToEtaAndXi(potentialPositionsArrayList, FishList["742"]);

            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();

            double depth = extractDataFromEtaAndXi.getDepth(etaAndXi[0], etaAndXi[1], ds["h"].GetData());
            Console.WriteLine("depth: " + depth);

            double temp = extractDataFromEtaAndXi.getTemp(0, 0, etaAndXi[0], etaAndXi[1], ds["temp"].GetData());
            Console.WriteLine("temp: " + temp);

            //DataSet ds = DataSet.Open(@"C:\NCdata\BW.nc");
            //Console.WriteLine(ds);
            System.Console.ReadLine();
        }
    }
}
