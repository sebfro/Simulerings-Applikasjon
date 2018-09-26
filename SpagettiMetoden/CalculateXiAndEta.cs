using System;
using System.Collections;

namespace SpagettiMetoden
{
    class CalculateXiAndEta
    {
        public const int Eta_Rho = 580;
        public const int Xi_Rho = 1202;
        public const double Delta = 0.1;

        public static PositionData GeneratePositionDataArrayList(Array latDataSet, Array lonDataSet, double lat, double lon)
        {
            
            ArrayList potentialPositionArray = new ArrayList();

            for (int i = 0; i < GlobalVariables.eta_rho_size; i++)
            {
                for (int j = 0; j < GlobalVariables.xi_rho_size; j++)
                {
                        if (Math.Abs((double)latDataSet.GetValue(i, j) - lat) < Delta && Math.Abs((double)lonDataSet.GetValue(i, j) - lon) < Delta)
                        {
                            potentialPositionArray.Add(new PositionData(i, j, (double)latDataSet.GetValue(i, j), (double)lonDataSet.GetValue(i, j)));
                        }
                }
            }

            return ConvertLatAndLonToEtaAndXi(potentialPositionArray, lat, lon);
        }

        public static PositionData ConvertLatAndLonToEtaAndXi(ArrayList potentialPositionsArrayList, double lat, double lon)
        {
            double minDelta = 0;
            bool deltaHasBeenSet = false;

            PositionData positionData = new PositionData(0, 0, 0.0, 0.0);

            foreach (PositionData pData in potentialPositionsArrayList)
            {
                double newDelta = Math.Abs(pData.lat - lat) + Math.Abs(pData.lon - lon);
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

                //Console.WriteLine("lat: " + pData.lat + ", lon: " + pData.lon + ". eta_rho: " + pData.eta_rho + ", xi_rho: " + pData.xi_rho + ". minDelta: " + minDelta);
            }

            //Console.WriteLine("minDelta: " + minDelta + ", eta_rho: " + positionData.eta_rho + ", xi_rho: " + positionData.xi_rho);

            return positionData;
        }
    }
}
