using System;
using Microsoft.Research.Science.Data;

namespace SpagettiMetoden
{
    class HeatMap
    {
        //year og month trengs kun når det er flere netCDF filer som må leses inn
        //public string year { get; set; }
        //public string month { get; set; }
        public Array LatArray { get; set;}
        public Array LonArray { get; set;}
        public bool Initialized { get; set; }

        public HeatMap()
        {
            //this.year = year;
            //this.month = month;
            DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMaps);
            Console.WriteLine(GlobalVariables.pathToNcHeatMaps);
            LatArray = ds["lat_rho"].GetData();
            LonArray = ds["lon_rho"].GetData();
            Initialized = true;
        }
        
    }
}
