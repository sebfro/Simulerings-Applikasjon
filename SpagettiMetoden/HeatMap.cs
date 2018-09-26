using System;
using Microsoft.Research.Science.Data;

namespace SpagettiMetoden
{
    class HeatMap
    {
        public string year { get; set; }
        public string month { get; set; }
        public DataSet ds { get; set; }
        public Array latArray { get; set;}
        public Array lonArray { get; set;}
        //public Array ocean_time { get; set;}
        public Array depthArray { get; set;}
        public bool initialized { get; set; }

        public HeatMap(string year, string month)
        {
            this.year = year;
            this.month = month;
            ds = DataSet.Open(GlobalVariables.pathToNcHeatMaps + year + month + "01.nc");
            Console.WriteLine(GlobalVariables.pathToNcHeatMaps + year + month + "01.nc");
            latArray = ds["lat_rho"].GetData();
            lonArray = ds["lon_rho"].GetData();
            //Array ocean_time = ds["ocean_time"].GetData();
            depthArray = ds["h"].GetData();
            initialized = true;
        }
        public HeatMap()
        {
            initialized = false;
        }
    }
}
