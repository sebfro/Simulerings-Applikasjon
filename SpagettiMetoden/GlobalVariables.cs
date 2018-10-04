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
        public static int releasedFish = 300;
        public static string pathToNcHeatMaps = @"I:\VarmeModell\norkyst_800m_avg.nc";
        public static string pathToNcHeatMapFolder = @"I:\VarmeModell\";
        public static string pathToNcTagData = @"I:\Merkedata\";
        //ALL THE DELTAS:
        public static double TempDelta = 1.5;
        public static double DepthDelta = 10;

        public static int tagStep = 576;
        //Hvor langt fisken beveger seg per iterasjon
        public static int increment = 40;
        public static int increment2 = 85;

        //Hvor mange dager per iterasjon
        public static int dayIncrement = 4;

        //Først dag i merkedataen
        public static int day = 27;

        //Sannsynlighet for å velge en path som er nærmere "capture point"
        public static double probability = 0.7;

    }

    public static class ThreadSafeRandom
    {
        private static Random _inst = new Random();

        public static int Next(int range)
        {
            lock (_inst) return _inst.Next(range);
        }
        public static double NextDouble()
        {
            lock (_inst) return _inst.NextDouble();
        }
    }
}
