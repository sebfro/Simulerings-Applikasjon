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
        public bool valid { get; set; }

        public PositionData(int xi_rho, int eta_rho, double lon, double lat)
        {
            this.xi_rho = xi_rho;
            this.eta_rho = eta_rho;
            this.lon = lon;
            this.lat = lat;
            valid = true;
        }

        public bool isValid()
        {
            return valid;
        }
    }
}
