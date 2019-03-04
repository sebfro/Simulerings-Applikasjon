using MathNet.Numerics;
using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace SpagettiMetoden
{
    class TempContainer
    {
        private static readonly int add_offset = 10;
        private static readonly double scale_factor = 0.001;

        public Array norkystTempArray;
        public Array barentsTempArray;
        

        public string norkystPath;
        public string barentsPath;

        public ConcurrentQueue<Array> HeatMapQueue { get; set; }

        public List<TagData> tagDatas;
        public int tagStep;
        public int progress;

        public bool norkystExists = true;
        public bool barentsExists = true;

        

        public TempContainer(List<TagData> tagDatas, int tagStep)
        {
            progress = 0;
            this.tagDatas = tagDatas;
            this.tagStep = tagStep;
            HeatMapQueue = new ConcurrentQueue<Array>();
            

            norkystPath = GlobalVariables.pathToNorkystNetCDF;
            barentsPath = GlobalVariables.pathToOceanAvgNetCDF;

            //Thread thread = new Thread(test);
            //thread.Start();
            //UpdateTempArray(GlobalVariables.startDate);


            norkystExists = CheckIfNetCdfAreAvailable(norkystPath);
            barentsExists = CheckIfNetCdfAreAvailable(barentsPath);
            if(!norkystExists && !barentsExists)
            {
                throw new FileNotFoundException();
            } else if(norkystExists || barentsExists)
            {
                GlobalVariables.allow_switching = false;
            }
        }

        public bool CheckIfNetCdfAreAvailable(string path)
        {
            return File.Exists(path + tagDatas[tagDatas.Count / 2].Date + ".nc");
        }

        public void test()
        {
            DataSet ds;
            for (int i = 0; i < tagDatas.Count; i += tagStep)
            {
                ds = DataSet.Open(norkystPath + tagDatas[i].Date + ".nc");
                HeatMapQueue.Enqueue(ds["temp"].GetData());
            }
        }

        public void LoadHeatMapsForNextMonth()
        {
            DataSet ds;
            progress = progress + (30 * tagStep);
            for (int i = progress; i < tagDatas.Count && i < progress; i += tagStep)
            {
                ds = DataSet.Open(norkystPath + tagDatas[i].Date + ".nc");
                HeatMapQueue.Enqueue(ds["temp"].GetData());
                //progress = i;
            }
            progress += tagStep;
            
        }

        public void GetHeatMap(string date)
        {
            /*if (int.Parse(date.Substring(4, 2)) != month)
            {
                month = int.Parse(date.Substring(4, 2));
                Thread thread = new Thread(LoadHeatMapsForNextMonth);
                thread.Start();
                
            }*/

            while (HeatMapQueue.IsEmpty);

            if(HeatMapQueue.TryDequeue(out Array array))
            {
                norkystTempArray = array;
            }
        }

        public void UpdateTempArray(string date)
        {
            DataSet ds;
            if (norkystExists)
            {
                ds = DataSet.Open(norkystPath + date + ".nc");
                norkystTempArray = ds["temp"].GetData();
            }
            if (barentsExists)
            {
                ds = DataSet.Open(barentsPath + date + ".nc");
                barentsTempArray = ds["temp"].GetData();
            }
        }
        

        /// <summary>
        /// scale_factor og add_offset brukes her for å konvertere en int16
        /// tilbake til en double verdi slik at vi kan bruke den
        /// </summary>
        /// <param name="z_rho"></param>
        /// <param name="eta_rho"></param>
        /// <param name="xi_rho"></param>
        /// <returns>temperatur fra en en array4D av temperatur</returns>
        public double GetTemp(int z_rho, int eta_rho, int xi_rho, bool use_norkyst)
        {
            double tempValue = 0;
            if (use_norkyst)
            {
                tempValue = (short)norkystTempArray.GetValue(z_rho, eta_rho, xi_rho);
            } else
            {
                tempValue = (short)barentsTempArray.GetValue(z_rho, eta_rho, xi_rho);
            }
                return (tempValue * scale_factor) + add_offset;
        }

        public class EtaXiCase
        {
            public int eta = 0;
            public int xi = 0;
            public bool seaCurrentDrag = false;

            public EtaXiCase(int eta, int xi, bool seaCurrentDrag)
            {
                this.seaCurrentDrag = seaCurrentDrag;
                this.eta = eta;
                this.xi = xi;
            }
            public EtaXiCase(int eta, int xi)
            {
                this.eta = eta;
                this.xi = xi;
            }
        }
        

        public readonly EtaXiCase[] EtaXiCases = new EtaXiCase[]
        {
            new EtaXiCase(1, -1),   //0
            new EtaXiCase(1, 0),    //1
            new EtaXiCase(1, 1),    //2
            new EtaXiCase(0, -1),   //3
            new EtaXiCase(-1, -1),  //4
            new EtaXiCase(-1, 0),   //5
            new EtaXiCase(-1, 1),   //6
            new EtaXiCase(0, 1)     //7
        };


        

        public double ConvertToRho(double var1, double var2)
        {
            return (var1 + var2) / 2;
        }

        public double GetCurrentValue(Array current, int z_rho, int eta, int xi)
        {
            return scale_factor * (short)current.GetValue(z_rho, eta, xi);
        }
        

    }
}
