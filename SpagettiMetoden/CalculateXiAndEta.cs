using System;
using System.Collections;

namespace SpagettiMetoden
{
    class CalculateXiAndEta
    {
        public const int Eta_Rho = 580;
        public const int Xi_Rho = 1202;
        public const double Delta = 0.1;

        public static PositionData GeneratePositionDataArrayList(Array NorkystlatDataSet, Array NorKystlonDataSet,
            Array BarentsSealatDataSet, Array BarentsSealatlonDataSet, double lat, double lon)
        {
            
            ArrayList potentialPositionArray = new ArrayList();

            if (GlobalVariables.use_ocean_time)
            {
                for (int i = 0; i < GlobalVariables.eta_rho_size_ocean_time; i++)
                {
                    for (int j = 0; j < GlobalVariables.xi_rho_size_ocean_time; j++)
                    {
                        if (Math.Abs((double)NorkystlatDataSet.GetValue(i, j) - lat) < Delta && Math.Abs((double)NorKystlonDataSet.GetValue(i, j) - lon) < Delta)
                        {
                            potentialPositionArray.Add(new PositionData(i, j, (double)NorkystlatDataSet.GetValue(i, j), (double)NorKystlonDataSet.GetValue(i, j), true));
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < GlobalVariables.eta_rho_size_ocean_avg; i++)
                {
                    for (int j = 0; j < GlobalVariables.xi_rho_size_ocean_avg; j++)
                    {
                        if (Math.Abs((double)BarentsSealatDataSet.GetValue(i, j) - lat) < Delta && Math.Abs((double)BarentsSealatlonDataSet.GetValue(i, j) - lon) < Delta)
                        {
                            potentialPositionArray.Add(new PositionData(i, j, (double)BarentsSealatDataSet.GetValue(i, j), (double)BarentsSealatlonDataSet.GetValue(i, j), false));
                        }
                    }
                }
            }
            

            return ConvertLatAndLonToEtaAndXi(potentialPositionArray, lat, lon);
        }

        public static PositionData ConvertLatAndLonToEtaAndXi(ArrayList potentialPositionsArrayList, double lat, double lon)
        {
            double minDelta = 0;
            bool deltaHasBeenSet = false;

            PositionData positionData = new PositionData(0, 0, 0.0, 0.0, true);

            foreach (PositionData pData in potentialPositionsArrayList)
            {
                double newDelta = Math.Abs(pData.Lat - lat) + Math.Abs(pData.Lon - lon);
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
            }
            return positionData;
        }
    }
}
