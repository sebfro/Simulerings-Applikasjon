using Microsoft.Research.Science.Data;
using System;
using System.Collections;

namespace SpagettiMetoden
{
    class ExtractDataFromEtaAndXi
    {
        public Array OceanTime_DepthArray { get; set; }
        public Array OceanAvg_DepthArray { get; set; }
        public Array OceanTime_Z_Array { get; set; }
        public Array OceanAvg_Z_Array { get; set; }
        //mask_rho brukes til å sjekke om et eta og xi punkt er på land eller ikke
        //0.0 for land og 1.0 for hav
        public Array OceanTime_Mask_rhoArray { get; set; }
        public Array OceanAvg_Mask_rhoArray { get; set; }

        public int DepthDelta { get; set; }

        public ExtractDataFromEtaAndXi(int depthDelta)
        {
            DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMapOcean_Time);
            OceanTime_DepthArray = ds["h"].GetData();
            OceanTime_Mask_rhoArray = ds["mask_rho"].GetData();
            OceanTime_Z_Array = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NK800_Z.nc")["Z"].GetData();

            ds = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "mndmean_avg_200810.nc");
            OceanAvg_DepthArray = ds["h"].GetData();
            OceanAvg_Mask_rhoArray = ds["mask_rho"].GetData();
            OceanAvg_Z_Array = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NS4MI_Z.nc")["Z"].GetData();
            DepthDelta = depthDelta;
        }

        //Sjekker om et eta og xi punkt er på land. Returnerer true hvis det er på land, false hvis punktet
        //er i havet
        public bool IsOnLand(int eta_rho, int xi_rho)
        {
            //eta_rho -= 1;
            //xi_rho -= 1;
            if (GlobalVariables.use_ocean_time)
            {
                return (double)OceanTime_Mask_rhoArray.GetValue(eta_rho, xi_rho) == 0.0;
            }
            else
            {
                return (double)OceanAvg_Mask_rhoArray.GetValue(eta_rho, xi_rho) == 0.0;
            }
        }

        public double GetDepth(int eta_rho, int xi_rho)
        {
            //eta_rho = eta_rho == 0 ? eta_rho : eta_rho - 1;
            //xi_rho = xi_rho == 0 ? xi_rho : xi_rho - 1;
            if (GlobalVariables.use_ocean_time)
            {
                return (double)OceanTime_DepthArray.GetValue(eta_rho, xi_rho);
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
        public DepthData GetS_rhoValues(int eta_rho, int xi_rho, double tagDataDepth)
        {
            // the code that you want to measure comes here
            
            ArrayList potentialDepthArray = new ArrayList();
            int z_rho_size;
            Array z_Array;

            if (GlobalVariables.use_ocean_time)
            {
                z_rho_size = GlobalVariables.Z_rho_size_ocean_time;
                z_Array = OceanTime_Z_Array;
            }
            else
            {
                z_rho_size = GlobalVariables.Z_rho_size_ocean_avg;
                z_Array = OceanAvg_Z_Array;
            }
            for (int k = 0; k < z_rho_size; k++)
            {
                double depthFromZ_rho = (double)z_Array.GetValue(k, eta_rho, xi_rho);
                
                if (Math.Abs (depthFromZ_rho - tagDataDepth) < DepthDelta)
                {
                    potentialDepthArray.Add(new DepthData(k, depthFromZ_rho));
                }
            }
            //Sammenligne eta_rho og xi_rho fra de potensielle dybdene med den faktiske dybden og velge denn med minst differanse

            double minDelta = 0.0;
            bool deltaHasBeenSet = false;
            DepthData depthData = new DepthData(0, 0.0);

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
            if(!deltaHasBeenSet)
            {
                depthData.Valid = false;
            }
            return depthData;
        }
        
    }

    class DepthData
    {
        public double Depth { get; set; }
        public int Z_rho { get; set; }
        public bool Valid { get; set; }

        public DepthData(int z_rho, double depth)
        {
            Depth = depth;
            Z_rho = z_rho;
            Valid = true;
        }

    }
}
