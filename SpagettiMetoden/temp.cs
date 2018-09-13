using MathNet.Numerics;
using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class temp
    {
        public void calc(DataSet ds, string stagger, int vTransform)
        {
            var H2d = ds["h"].GetData();
            var C = ds["Cs_r"].GetData();
            var Hc = ds["hc"].GetData();
            var s_rho = ds["s_rho"].GetData();

            double[] H1d = new double[H2d.Length];
            Buffer.BlockCopy(H2d, 0, H1d, 0, H2d.Length);

            int N = C.Length;

            double[] S;

            if(stagger == "rho")
            {
                S = (double[])s_rho;
            } else if (stagger == "w")
            {
                S = Generate.LinearSpaced(-1, 0.0, N);
            }

            if(vTransform == 1)
            {
                //var A = Hc * (S - C)
            }

            foreach (double hc in C)
            {
                Console.WriteLine("Hc: " + C);
            }
        }

    }
}
