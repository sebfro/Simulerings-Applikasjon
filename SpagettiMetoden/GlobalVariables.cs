using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    public class GlobalVariables
    {
        public static int eta_rho_size = 902;
        public static int xi_rho_size = 2602;
        public static int s_rho_size = 35;
        public static int Z_rho_size = 35;
        public static int releasedFish = 100;
        public static string pathToNcHeatMaps = @"I:\VarmeModell\norkyst_800m_avg.nc";
        public static string pathToNcHeatMapFolder = @"I:\VarmeModell\";
        public static string pathToNcTagData = @"I:\Merkedata\";
        //Ikke implemetert ennå
        public static double Delta = 0.1;
    }
}
