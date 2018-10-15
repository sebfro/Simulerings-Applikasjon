using System;
using System.Collections;

namespace SpagettiMetoden
{
    class ExtractDataFromEtaAndXi
    {
        public Array DepthArray { get; set; }
        public Array Z_Array { get; set; }
        //mask_rho brukes til å sjekke om et eta og xi punkt er på land eller ikke
        //0.0 for land og 1.0 for hav
        public Array Mask_rhoArray { get; set; }

        public int DepthDelta { get; set; }

        public ExtractDataFromEtaAndXi(Array DepthArray, Array Z_Array, Array Mask_rhoArray, int depthDelta)
        {
            this.DepthArray = DepthArray;
            this.Z_Array = Z_Array;
            this.Mask_rhoArray = Mask_rhoArray;
            DepthDelta = depthDelta;
        }

        //Sjekker om et eta og xi punkt er på land. Returnerer true hvis det er på land, false hvis punktet
        //er i havet
        public bool IsOnLand(int eta_rho, int xi_rho)
        {
            return (double)Mask_rhoArray.GetValue(eta_rho-1, xi_rho-1) == 0.0;
        }

        public double GetDepth(int eta_rho, int xi_rho)
        {

            return (double)DepthArray.GetValue(eta_rho-1, xi_rho-1);
        }

        public double getLatOrLon(int eta_rho, int xi_rho, Array latOrLonArray)
        {
            return (double)latOrLonArray.GetValue(eta_rho-1, xi_rho-1);
        }

        public DepthData getS_rhoValues(int eta_rho, int xi_rho, double tagDataDepth)
        {
            // the code that you want to measure comes here
            
            ArrayList potentialDepthArray = new ArrayList();

            for (int k = 0; k < GlobalVariables.Z_rho_size; k++)
            {
                double depthFromZ_rho = (double)Z_Array.GetValue(k, eta_rho-1, xi_rho-1);
                
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
