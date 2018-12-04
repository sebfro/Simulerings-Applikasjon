using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class PositionData
    {
        public int Xi_rho { get; set; }
        public int Eta_rho { get; set; }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public double Depth { get; set; }
        public double Temp { get; set; }
        public double TagDataDepth { get; set; }
        public double TagDataTemp { get; set; }
        public bool Alive { get; set; }
        public bool ExtraWeigth { get; set; }

        public PositionData(int eta_rho, int xi_rho, double lat, double lon)
        {
            Xi_rho = xi_rho;
            Eta_rho = eta_rho;
            Lon = lon;
            Lat = lat;
            Depth = 0;
            Temp = 0;
            Alive = true;
        }

        public PositionData(double lat, double lon)
        {
            Xi_rho = 0;
            Eta_rho = 0;
            Lon = lon;
            Lat = lat;
            Depth = 0;
            Temp = 0;
            Alive = true;
        }
        public PositionData(double lat, double lon, double depth, double temp, double tagDataDepth, double tagDataTemp, int eta_rho, int xi_rho)
        {
            Xi_rho = xi_rho;
            Eta_rho = eta_rho;
            Lon = lon;
            Lat = lat;
            Depth = depth;
            Temp = temp;
            TagDataDepth = tagDataDepth;
            TagDataTemp = tagDataTemp;
            Alive = true;
            ExtraWeigth = false;
        }

        public PositionData(double lat, double lon, double depth, double temp, double tagDataDepth, double tagDataTemp, int eta_rho, int xi_rho, bool extraWeigth)
        {
            Xi_rho = xi_rho;
            Eta_rho = eta_rho;
            Lon = lon;
            Lat = lat;
            Depth = depth;
            Temp = temp;
            TagDataDepth = tagDataDepth;
            TagDataTemp = tagDataTemp;
            Alive = true;
            ExtraWeigth = extraWeigth;
        }
        public PositionData() { }
        
    }
}
