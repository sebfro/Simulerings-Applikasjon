using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class PositionData
    {
        public int xi_rho { get; set; }
        public int eta_rho { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
        public double depth { get; set; }
        public double temp { get; set; }
        public double tagDataDepth { get; set; }
        public double tagDataTemp { get; set; }
        public bool alive { get; set; }

        public PositionData(int eta_rho, int xi_rho, double lat, double lon)
        {
            this.xi_rho = xi_rho;
            this.eta_rho = eta_rho;
            this.lon = lon;
            this.lat = lat;
            depth = 0;
            temp = 0;
            alive = true;
        }

        public PositionData(double lat, double lon, double depth, double temp, double tagDataDepth, double tagDataTemp)
        {
            xi_rho = 0;
            eta_rho = 0;
            this.lon = lon;
            this.lat = lat;
            this.depth = depth;
            this.temp = temp;
            this.tagDataDepth = tagDataDepth;
            this.tagDataTemp = tagDataTemp;
            alive = true;
        }
        
    }
}
