using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    public class GlobalVariables
    {
        /// <summary>
        /// eta_rho, xi_rho, s_rho og z_rho for norkyst
        /// varmemodellene
        /// </summary>
        public static int eta_rho_size_norkyst = 902;
        public static int xi_rho_size_norkyst = 2602;
        public static int s_rho_size_norkyst = 35;
        public static int Z_rho_size_norkyst = 35;
        /// <summary>
        /// eta_rho, xi_rho, s_rho og z_rho for barentshavet
        /// varmemodellene
        /// </summary>
        public static int eta_rho_size_ocean_avg = 580;
        public static int xi_rho_size_ocean_avg = 1202;
        public static int s_rho_size_ocean_avg = 32;
        public static int Z_rho_size_ocean_avg = 32;

        //Begynn med norkyst hvis true, barentshavet hvis false:
        //Husk å endre startdato hvis du skal flippe denne
        public static bool use_norkyst = false;

        public static int releasedFish = 10000;
        public static string basePath = @"D:\NCdata\";
        //Adressen til SSDen, bruk basePath for the som ligger på en hdd
        public static string PerformanceBasePath = @"C:\NCdata\";
        public static string ExternalHDDBasePath = @"H:\";
        public static string pathToNorkystNetCDF = basePath + @"Nye varmeModeller\Norkyst\Uten Havstrom\norkyst_";
        public static string pathToOceanAvgNetCDF = basePath + @"Nye varmemodeller\Ocean_Avg\Uten havstrom\ocean_avg_";
        public static string pathToNcHeatMapFolder = basePath + @"VarmeModell\";
        public static string pathToNcHeatMapnorkyst = basePath + @"VarmeModell\norkyst_800m_avg.nc";
        public static string pathToNcHeatMapOcean_Avg = basePath + @"VarmeModell\" + "ocean_avg_LatAndLonRho.nc";//basePath + @"ocean_avg_LatAndLonRho.nc";
        public static string pathToNcTagData = basePath + @"Merkedata\";
        public static string pathToMergedDirectory = PerformanceBasePath + @"fishData\";
        //Husk å endre, visser for rutene som algoritmen finner skal lagres
        public static string pathToSaveFishData = PerformanceBasePath + @"fishData";
        //ALL THE DELTAS:
        public static double TempDelta = 1;
        public static double DepthDelta = 10;
        public static bool allow_switching = true;


        //norkyst max value, denne er for Norkyst. Den går fra 1-274 (0-273 i kode)
        public static int norkyst_Max = 274;

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
        public static string startDate = "20030830";
        public static int day = 29;
        public static int lastDay = 225;

        //Sannsynlighet for å velge en path som er nærmere "capture point"
        public static double Probability { get; set; }


    }
    
}