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
        public static int add_offset = 10;
        public static double scale_factor = 0.001;
        public Dictionary<int, Array> tempDictionary = new Dictionary<int, Array>();
        public int day;
        public object syncObject = new object();

        public Array tempArray;
        public Array tempArray2;

        public Array seaCurrentArrayU;
        public Array seaCurrentArrayV;
        public Array anglesArray;

        public string year;
        public int month;
        public string currDay;

        public string basePath;
        public bool use_ocean_time;

        public ConcurrentQueue<Array> HeatMapQueue { get; set; }

        public List<TagData> tagDatas;
        public int tagStep;
        public int progress;

        public TempContainer(List<TagData> tagDatas, int tagStep)
        {
            progress = 0;
            this.tagDatas = tagDatas;
            this.tagStep = tagStep;
            HeatMapQueue = new ConcurrentQueue<Array>();
            //Thread thread = new Thread(test);
            
            anglesArray = DataSet.Open(GlobalVariables.pathToNcHeatMapOcean_Time)["angle"].GetData();
            use_ocean_time = GlobalVariables.use_ocean_time;
            if (use_ocean_time)
            {
                basePath = GlobalVariables.pathToOceanTimeNetCDF;
            } else
            {
                basePath = GlobalVariables.pathToOceanAvgNetCDF;
            }
            month = int.Parse(GlobalVariables.startDate.Substring(4, 2));
            //thread.Start();
            //UpdateTempArray(GlobalVariables.startDate);
            
        }

        public void SetBasePath(bool b)
        {
            use_ocean_time = b;
            if (b)
            {
                basePath = GlobalVariables.pathToOceanTimeNetCDF;
            }
            else
            {
                basePath = GlobalVariables.pathToOceanAvgNetCDF;
            }
        }

        public void test()
        {
            DataSet ds;
            for (int i = progress; i < tagDatas.Count && int.Parse(tagDatas[progress].Date.Substring(4, 2)) == month; i += tagStep)
            {
                ds = DataSet.Open(basePath + tagDatas[i].Date + ".nc");
                HeatMapQueue.Enqueue(ds["temp"].GetData());
                progress = i;
            }
            progress += tagStep;
            
        }

        public void test2(string date)
        {
            if (int.Parse(date.Substring(4, 2)) != month)
            {
                month = int.Parse(date.Substring(4, 2));
                Thread thread = new Thread(test);
                thread.Start();
                
            }

            while (HeatMapQueue.IsEmpty)
            {

            }

            if(HeatMapQueue.TryDequeue(out Array array))
            {
                tempArray = array;
            }
        }

        public void UpdateTempArray(string date)
        {
            DataSet ds = DataSet.Open(basePath + date + ".nc");
            tempArray = ds["temp"].GetData();
        }

        public void UpdateDay(int day)
        {
            this.day = day;
        }

        /// <summary>
        /// scale_factor og add_offset brukes her for å konvertere en int16
        /// tilbake til en double verdi slik at vi kan bruke den
        /// </summary>
        /// <param name="z_rho"></param>
        /// <param name="eta_rho"></param>
        /// <param name="xi_rho"></param>
        /// <returns>temperatur fra en en array4D av temperatur</returns>
        public double GetTemp(int z_rho, int eta_rho, int xi_rho)
        {
                double tempValue = (short)tempArray.GetValue(z_rho, eta_rho, xi_rho);
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


        //private EtaXiCase[,] EtaXiCases { get => etaXiCases; set => etaXiCases = value; }

        public EtaXiCase[] GetPositionsToCheck(int z_rho, int eta, int xi)
        {
            EtaXiCase[] etaXiCases = EtaXiCases;

            eta = eta - 1;
            xi = xi - 1;
            double uValue1 = GetCurrentValue(seaCurrentArrayU, z_rho, eta, xi - 1);
            double uValue2 = GetCurrentValue(seaCurrentArrayU, z_rho, eta , xi- 1);
            double vValue1 = GetCurrentValue(seaCurrentArrayV, z_rho, eta - 1, xi);
            double vValue2 = GetCurrentValue(seaCurrentArrayV, z_rho, eta - 1, xi);

            double U_rho = ConvertToRho(uValue1, uValue2);
            double V_rho = ConvertToRho(vValue1, vValue2);

            double angle = (double)anglesArray.GetValue(eta, xi);
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            double U_rot = (U_rho * cosAngle) - (V_rho * sinAngle);
            double V_rot = (V_rho * cosAngle) + (U_rho * sinAngle);

            if (U_rot > 0 && V_rot > 0)
            {

                //EtaXiCase[0, 0] = 1;
                //EtaXiCase[0, 1] = 1;
                etaXiCases[2].seaCurrentDrag = true;

                //EtaXiCase[1, 0] = 0;
                //EtaXiCase[1, 1] = 1;
                etaXiCases[7].seaCurrentDrag = true;
                //EtaXiCase[2, 0] = 1;
                //EtaXiCase[2, 1] = 0;
                etaXiCases[1].seaCurrentDrag = true;

            } else if (U_rot < 0 && V_rot < 0)
            {
                //EtaXiCase[0, 0] = -1;
                //EtaXiCase[0, 1] = -1;
                etaXiCases[4].seaCurrentDrag = true;
                //EtaXiCase[1, 0] = 0;
                //EtaXiCase[1, 1] = -1;
                etaXiCases[3].seaCurrentDrag = true;
                //EtaXiCase[2, 0] = -1;
                //EtaXiCases[2, 1] = 0;
                etaXiCases[5].seaCurrentDrag = true;
            }
            else if (U_rot > 0 && V_rot < 0)
            {
                //EtaXiCases[0, 0] = 1;
                //EtaXiCases[0, 1] = -1;
                etaXiCases[0].seaCurrentDrag = true;
                //EtaXiCases[1, 0] = 0;
                //EtaXiCases[1, 1] = -1;
                etaXiCases[3].seaCurrentDrag = true;
                //EtaXiCases[2, 0] = 1;
                //EtaXiCases[2, 1] = 0;
                etaXiCases[1].seaCurrentDrag = true;
            }
            else if (U_rot < 0 && V_rot > 0)
            {
                //EtaXiCases[0, 0] = -1;
                //EtaXiCases[0, 1] = 1;
                etaXiCases[6].seaCurrentDrag = true;
                //EtaXiCases[1, 0] = 0;
                //EtaXiCases[1, 1] = 1;
                etaXiCases[7].seaCurrentDrag = true;
                //EtaXiCases[2, 0] = -1;
                //EtaXiCases[2, 1] = 0;
                etaXiCases[5].seaCurrentDrag = true;
            }
            return etaXiCases;
        }

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
