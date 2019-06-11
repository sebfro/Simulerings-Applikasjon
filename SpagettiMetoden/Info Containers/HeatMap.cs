using System;
using Microsoft.Research.Science.Data;

namespace SimuleringsApplikasjonen
{
    class HeatMap
    {
        //year og month trengs kun når det er flere netCDF filer som må leses inn
        //public string year { get; set; }
        //public string month { get; set; }
        public Array BarentsSeaLatArray { get; set; }
        public Array BarentsSeaLonArray { get; set; }
        public Array NorKystLatArray { get; set;}
        public Array NorKystLonArray { get; set;}
        public bool Initialized { get; set; }

        public HeatMap()
        {
            //this.year = year;
            //this.month = month;
            DataSet ds_Ocean_Time = DataSet.Open(GlobalVariables.pathToNcHeatMapnorkyst);
            Console.WriteLine(GlobalVariables.pathToNcHeatMapnorkyst);
            NorKystLatArray = ds_Ocean_Time["lat_rho"].GetData();
            NorKystLonArray = ds_Ocean_Time["lon_rho"].GetData();

            
            DataSet ds_Ocean_Avg = DataSet.Open(GlobalVariables.pathToNcHeatMapOcean_Avg);
            Console.WriteLine(GlobalVariables.pathToNcHeatMapOcean_Avg);
            BarentsSeaLatArray = ds_Ocean_Avg["lat_rho"].GetData();
            BarentsSeaLonArray = ds_Ocean_Avg["lon_rho"].GetData();
            
            Initialized = true;
        }
        
    }
}
