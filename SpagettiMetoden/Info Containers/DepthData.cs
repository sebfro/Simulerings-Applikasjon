using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuleringsApplikasjonen.Info_Containers
{
    class DepthData
    {
        public double Depth { get; set; }
        public int Z_rho { get; set; }
        public bool Valid { get; set; }

        public DepthData(int z_rho, double depth)
        {
            Depth = depth;
            Z_rho = z_rho;
            Valid = true;
        }

    }
}
