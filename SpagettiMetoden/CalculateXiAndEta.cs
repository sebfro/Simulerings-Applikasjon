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

        public PositionData GeneratePositionDataArrayList(Array latDataSet, Array lonDataSet, Fish fish)
        {
            
            ArrayList potentialPositionArray = new ArrayList();

            for (int i = 0; i < GlobalVariables.eta_rho_size; i++)
            {
                for (int j = 0; j < GlobalVariables.xi_rho_size; j++)
                {
                        if (Math.Abs((double)latDataSet.GetValue(i, j) - fish.releaseLat) < Delta && Math.Abs((double)lonDataSet.GetValue(i, j) - fish.releaseLon) < Delta)
                        {
                            potentialPositionArray.Add(new PositionData(i, j, (double)latDataSet.GetValue(i, j), (double)lonDataSet.GetValue(i, j)));
                        }
                }
            }

            return ConvertLatAndLonToEtaAndXi(potentialPositionArray, fish);
        }
        //return an int[] with 2 var, index 0 is eta_rho and index 1 is xi_rho
        public PositionData ConvertLatAndLonToEtaAndXi(ArrayList potentialPositionsArrayList, Fish fish)
        {
            double minDelta = 0;
            bool deltaHasBeenSet = false;

            PositionData positionData = new PositionData(0, 0, 0.0, 0.0);

            foreach (PositionData pData in potentialPositionsArrayList)
            {
                double newDelta = Math.Abs(pData.lat - fish.releaseLat) + Math.Abs(pData.lon - fish.releaseLon);
                if (!deltaHasBeenSet)
                {
                    minDelta = newDelta;
                    positionData = pData;
                    deltaHasBeenSet = true;
                }
                if (newDelta < minDelta)
                {
                    minDelta = newDelta;
                    positionData = pData;
                }

                Console.WriteLine("lat: " + pData.lat + ", lon: " + pData.lon + ". eta_rho: " + pData.eta_rho + ", xi_rho: " + pData.xi_rho + ". minDelta: " + minDelta);
            }

            Console.WriteLine("minDelta: " + minDelta + ", eta_rho: " + positionData.eta_rho + ", xi_rho: " + positionData.xi_rho);

            return positionData;
        }
    }
}
