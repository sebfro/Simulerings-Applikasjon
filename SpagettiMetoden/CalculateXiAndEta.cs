using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Science.Data;

namespace SpagettiMetoden
{
    class CalculateXiAndEta
    {
        public const int Eta_Rho = 580;
        public const int Xi_Rho = 1202;
        public const double Delta = 0.1;

        public ArrayList GeneratePositionDataArrayList(Array latDataSet, Array lonDataSet, double lat, double lon)
        {
            
            ArrayList potentialPositionArray = new ArrayList();

            for (int i = 0; i < Eta_Rho; i++)
            {
                for (int j = 0; j < Xi_Rho; j++)
                {
                        if (Math.Abs((double)latDataSet.GetValue(i, j) - lat) < Delta && Math.Abs((double)lonDataSet.GetValue(i, j) - lon) < Delta)
                        {
                            potentialPositionArray.Add(new PositionData(i, j, (double)latDataSet.GetValue(i, j), (double)lonDataSet.GetValue(i, j)));
                        }
                }
            }
            return potentialPositionArray;
        }
        //return an int[] with 2 var, index 0 is eta_rho and index 1 is xi_rho
        public int[] ConvertLatAndLonToEtaAndXi(ArrayList potentialPositionsArrayList, Fish fish)
        {
            double minDelta = 0;
            bool deltaHasBeenSet = false;
            int[] etaAndXi = new int[2];
            foreach (PositionData pData in potentialPositionsArrayList)
            {
                double newDelta = Math.Abs(pData.lat - fish.releaseLat) + Math.Abs(pData.lon - fish.releaseLon);
                if (!deltaHasBeenSet)
                {
                    minDelta = newDelta;
                    deltaHasBeenSet = true;
                }
                if (newDelta < minDelta)
                {
                    minDelta = newDelta;
                    etaAndXi[0] = pData.eta_rho;
                    etaAndXi[1] = pData.xi_rho;
                }

                Console.WriteLine("lat: " + pData.lat + ", lon: " + pData.lon + ". eta_rho: " + pData.eta_rho + ", xi_rho: " + pData.xi_rho + ". minDelta: " + minDelta);
            }

            Console.WriteLine("minDelta: " + minDelta + ", eta_rho: " + etaAndXi[0] + ", xi_rho: " + etaAndXi[1]);

            return etaAndXi;
        }
    }
}
