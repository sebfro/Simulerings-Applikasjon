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
                            Console.WriteLine(latDataSet.GetValue(i, j));
                            potentialPositionArray.Add(new PositionData(i, j, (double)latDataSet.GetValue(i, j), (double)lonDataSet.GetValue(i, j)));
                        }
                }
            }
            return potentialPositionArray;
        }
    }
}
