using Microsoft.Research.Science.Data;
using SpagettiMetoden.Info_Containers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpagettiMetoden
{
    class ExtractDataFromEtaAndXi
    {
        private Array Norkyst_DepthArray { get; set; }
        private Array OceanAvg_DepthArray { get; set; }
        private Array Norkyst_Z_Array { get; set; }
        private Array OceanAvg_Z_Array { get; set; }
        //mask_rho brukes til å sjekke om et eta og xi punkt er på land eller ikke
        //0.0 for land og 1.0 for hav
        private Array Norkyst_Mask_rhoArray { get; set; }
        private Array OceanAvg_Mask_rhoArray { get; set; }

        public int DepthDelta { get; set; }

        public ExtractDataFromEtaAndXi(int depthDelta)
        {
            DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMapnorkyst);
            Norkyst_DepthArray = ds["h"].GetData();
            Norkyst_Mask_rhoArray = ds["mask_rho"].GetData();
            Norkyst_Z_Array = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NK800_Z.nc")["Z"].GetData();

            ds = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "mndmean_avg_200810.nc");
            OceanAvg_DepthArray = ds["h"].GetData();
            OceanAvg_Mask_rhoArray = ds["mask_rho"].GetData();
            OceanAvg_Z_Array = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NS4MI_Z.nc")["Z"].GetData();
            DepthDelta = depthDelta;
            
        }

        //Sjekker om et eta og xi punkt er på land. Returnerer true hvis det er på land, false hvis punktet
        //er i havet
        public bool IsOnLand(int eta_rho, int xi_rho, bool use_norkyst)
        {
            //eta_rho -= 1;
            //xi_rho -= 1;
            if (use_norkyst)
            {
                return (double)Norkyst_Mask_rhoArray.GetValue(eta_rho, xi_rho) == 0.0;
            }
            else
            {
                return (double)OceanAvg_Mask_rhoArray.GetValue(eta_rho, xi_rho) == 0.0;
            }
        }

        public double GetDepth(int eta_rho, int xi_rho, bool use_norkyst)
        {
            //eta_rho = eta_rho == 0 ? eta_rho : eta_rho - 1;
            //xi_rho = xi_rho == 0 ? xi_rho : xi_rho - 1;
            if (use_norkyst)
            {
                return (double)Norkyst_DepthArray.GetValue(eta_rho, xi_rho);
            }
            else
            {
                return (double)OceanAvg_DepthArray.GetValue(eta_rho, xi_rho);
            }
        }

        public double GetLatOrLon(int eta_rho, int xi_rho, Array latOrLonArray)
        {
            return (double)latOrLonArray.GetValue(eta_rho, xi_rho);
        }
        //TODO: Kombiner for løkkene i denne funksjonen
        public DepthData GetS_rhoValues(int eta_rho, int xi_rho, double tagDataDepth, bool use_norkyst)
        {
            // the code that you want to measure comes here

            List<DepthData> potentialDepthArray = new List<DepthData>();
            int z_rho_size;
            Array z_Array;

            if (use_norkyst)
            {
                z_rho_size = GlobalVariables.Z_rho_size_norkyst;
                z_Array = Norkyst_Z_Array;
            }
            else
            {
                z_rho_size = GlobalVariables.Z_rho_size_ocean_avg;
                z_Array = OceanAvg_Z_Array;
            }
            double minDelta = 0.0;
            bool deltaHasBeenSet = false;
            DepthData depthData = new DepthData(0, 0.0);
            for (int k = 0; k < z_rho_size; k++)
            {
                double depthFromZ_rho = (double)z_Array.GetValue(k, eta_rho, xi_rho);
                double newDelta = Math.Abs(depthFromZ_rho - (-tagDataDepth));

                if (!deltaHasBeenSet)
                {
                    minDelta = newDelta;
                    depthData = new DepthData(k, depthFromZ_rho);
                    potentialDepthArray.Add(depthData);
                    deltaHasBeenSet = true;
                }else if (Math.Abs (depthFromZ_rho - tagDataDepth) < DepthDelta)
                {
                    if (newDelta < minDelta)
                    {
                        minDelta = newDelta;
                        depthData = new DepthData(k, depthFromZ_rho);
                    }
                    potentialDepthArray.Add(new DepthData(k, depthFromZ_rho));
                }
            }
            //Har kombinert denne for løkken med den over
            //Sammenligne eta_rho og xi_rho fra de potensielle dybdene med den faktiske dybden og velge denn med minst differanse
            /*
            foreach (DepthData dData in potentialDepthArray)
            {
                    double newDelta = Math.Abs(dData.Depth - (-tagDataDepth));

                    if (!deltaHasBeenSet)
                    {
                        minDelta = newDelta;
                        depthData = dData;

                        deltaHasBeenSet = true;
                    }
                    
                    if( newDelta < minDelta)
                    {
                        minDelta =  newDelta;
                        depthData = dData;
                    }
            }
            */
            if (!deltaHasBeenSet)
            {
                depthData.Valid = false;
            }
            return depthData;
        }
        
    }



    
}
