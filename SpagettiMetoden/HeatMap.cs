using System;
using Microsoft.Research.Science.Data;

namespace SpagettiMetoden
{
    class HeatMap
    {
        //year og month trengs kun når det er flere netCDF filer som må leses inn
        //public string year { get; set; }
        //public string month { get; set; }
        public DataSet ds { get; set; }
        public Array latArray { get; set;}
        public Array lonArray { get; set;}
        public Array depthArray { get; set;}
        //mask_rho brukes til å sjekke om et eta og xi punkt er på land eller ikke
        //0.0 for land og 1.0 for hav
        public Array mask_rhoArray { get; set; }
        public bool initialized { get; set; }

        public HeatMap()
        {
            //this.year = year;
            //this.month = month;
            ds = DataSet.Open(GlobalVariables.pathToNcHeatMaps);
            Console.WriteLine(GlobalVariables.pathToNcHeatMaps);
            latArray = ds["lat_rho"].GetData();
            lonArray = ds["lon_rho"].GetData();
            //mask_rho brukes til å sjekke om et eta og xi punkt er på land eller ikke
            //0.0 for land og 1.0 for hav
            mask_rhoArray = ds["mask_rho"].GetData();
            depthArray = ds["h"].GetData();
            initialized = true;
        }
        /*
        public HeatMap()
        {
            initialized = false;
        }
        */
    }
}
