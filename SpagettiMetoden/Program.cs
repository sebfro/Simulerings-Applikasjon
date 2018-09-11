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

            foreach (string f in KeyList)
            {
                Console.WriteLine("lat: " + FishList[f].captureLat + ", lon: " + FishList[f].captureLon);
            }

            DataSet ds = DataSet.Open(@"C:\NCdata\VarmeModell\mndmean_avg_200309.nc");

            var latArray = ds["lat_rho"].GetData();
            var lonArray = ds["lon_rho"].GetData();

            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();

            ArrayList potentialPositionsArrayList = calculateXiAndEta.GeneratePositionDataArrayList(latArray, lonArray, FishList["742"].releaseLat, FishList["742"].releaseLon);

            foreach ( PositionData pData in potentialPositionsArrayList)
            {
                Console.WriteLine("lat: " + pData.lat + ", lon: " + pData.lon + ". eta_rho: " + pData.eta_rho + ", xi_rho: " + pData.xi_rho);
            }

            //DataSet ds = DataSet.Open(@"C:\NCdata\BW.nc");
            //Console.WriteLine(ds);
            System.Console.ReadLine();
        }
    }
}
