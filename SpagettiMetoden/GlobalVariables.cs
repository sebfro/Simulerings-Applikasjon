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
        public static int releasedFish = 10000;
        public static string pathToNcHeatMaps = @"C:\NCdata\VarmeModell\norkyst_800m_avg.nc";
        public static string pathToNcHeatMapFolder = @"C:\NCdata\VarmeModell\";
        public static string pathToNcTagData = @"C:\NCdata\Merkedata\";
        public static string pathToOceanTimeNetCDF = @"C:\NCdata\VarmeModell\ocean_time";
        //Husk å endre, visser for rutene som algoritmen finner skal lagres
        public static string pathToSaveFishData = @"C:\NCdata\fishData";
        //ALL THE DELTAS:
        public static double TempDelta = 1;
        public static double DepthDelta = 10;

        //Ocean_time max value, denne er for Norkyst. Den går fra 1-274 (0-273 i kode)
        public static int ocean_time_Max = 274;

        /// <summary>
        /// Endrer dewnne til 580, utifra utregningene mine er et døgin i 10 min merkedata 145 step i mellom
        /// Derfor skal tagstep hver 580 istedenfor (145*4 = 580)
        /// Testet også med tre dager (435)
        /// </summary>
        public static int tagStep = 580;
        //Hvor langt fisken beveger seg per iterasjon
        //Standard er 85 og 40'
        //Sekundær standard 45/75
        public static int increment = 12;
        public static int increment2 = 20;

        //Hvor mange dager per iterasjon
        public static int dayIncrement = 4;

        //Først dag i merkedataen
        /// <summary>
        /// Tester å plusse på dayIncrement, fordi day er første dagen med merkedata
        /// Men første posisjonen hvis skal beregne i iterasjon 0 er posisjonen etter
        /// release posisjon. derfor day + dayIncrement
        /// </summary>
        public static int day = 29;

        //Sannsynlighet for å velge en path som er nærmere "capture point"
        public static double Probability { get; set; }


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
        public static double RandomSpeed(float max)
        {
            lock (_inst) return _inst.NextDouble() * (max - 0.4) + 0.4;
        }
    }
}