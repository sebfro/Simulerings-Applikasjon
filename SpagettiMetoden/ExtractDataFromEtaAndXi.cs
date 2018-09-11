using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Science.Data;

namespace SpagettiMetoden
{
    class ExtractDataFromEtaAndXi
    {

        public double getDepth(int eta_rho, int xi_rho, Array depthArray)
        {
            Console.WriteLine("eta: " + (eta_rho - 1) + ", xi: " + (xi_rho - 1));
            return (double)depthArray.GetValue(309, 756);
        }
        //Vet ikke helt hvorfor, men double fungerer ikke. Det er en single som blir returnert fra netcdf filen
        //antar det er en single precision og derfor kan kun bli castet til float. En double kan ta mot den senere.
        public float getTemp(int ocean_time, int s_rho, int eta_rho, int xi_rho, Array tempArray)
        {
            return (float)tempArray.GetValue(ocean_time, s_rho, eta_rho, xi_rho);
        }


    }
}
