using MathNet.Numerics;
using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

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

        public Array seaCurrentArrayU;
        public Array seaCurrentArrayV;
        public Array anglesArray;

        public TempContainer()
        {
            //tempArray = DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + GlobalVariables.day + ".nc")["temp"].GetData();
            //tempArray = DataSet.Open(@"C:\NCData\VarmeModell\TestFiles\ocean_time" + GlobalVariables.day + ".nc")["temp"].GetData();
            /*
            day = GlobalVariables.day;
            for(int i = day; i <= 225; i += dayIncrement)
            {
                tempDictionary.Add(i, DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + i + ".nc")["temp"].GetData());
            }
            */
            //tempArray = DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + GlobalVariables.day + ".nc")["temp"].GetData();
            //Console.WriteLine("Success: Alle heat maps have been loaded");
            anglesArray = DataSet.Open(GlobalVariables.pathToNcHeatMaps)["angle"].GetData();
            UpdateTempArray(GlobalVariables.day);
        }

        public void UpdateDay(int day)
        {
            this.day = day;
        }
        //Setter tempArray til den korrekte dagen for neste iterasjon, samme som før.
        //Men den hentes ut av en dictionary i rammen istedenfor fra hdd/ssd
        public void UpdateTempArray(double day)
        {
            //out tempArray setter variablen tempArray til det vi får ut av TryGetValue
            //tempDictionary.TryGetValue(day, out tempArray);
            DataSet ds = DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + (int)day + ".nc");
            tempArray = ds["temp"].GetData();
            seaCurrentArrayU = ds["u"].GetData();
            seaCurrentArrayV = ds["v"].GetData();
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
                eta_rho -= 1;
                xi_rho -= 1;

                double tempValue = (short)tempArray.GetValue(z_rho, eta_rho, xi_rho);
                return (tempValue * scale_factor) + add_offset;
        }

        public int[,] GetPositionsToCheck(int z_rho, int eta, int xi)
        {
            int[,] EtaXiCases = new int[3,2];

            eta = eta - 1;
            xi = xi - 1;
            double uValue1 = GetCurrentValue(seaCurrentArrayU, z_rho, eta, xi);
            double uValue2 = GetCurrentValue(seaCurrentArrayU, z_rho, eta, xi + 1);
            double vValue1 = GetCurrentValue(seaCurrentArrayV, z_rho, eta, xi);
            double vValue2 = GetCurrentValue(seaCurrentArrayV, z_rho, eta + 1, xi);

            double U_rho = ConvertToRho(uValue1, uValue2);
            double V_rho = ConvertToRho(vValue1, vValue2);

            double angle = (double)anglesArray.GetValue(eta, xi);
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            double U_rot = (U_rho * cosAngle) - (V_rho * sinAngle);
            double V_rot = (V_rho * cosAngle) + (U_rho * sinAngle);

            if (U_rot > 0 && V_rot > 0)
            {
                EtaXiCases[0, 0] = 1;
                EtaXiCases[0, 1] = 1;

                EtaXiCases[1, 0] = 0;
                EtaXiCases[1, 1] = 1;

                EtaXiCases[2, 0] = 1;
                EtaXiCases[2, 1] = 0;
            } else if (U_rot < 0 && V_rot < 0)
            {
                EtaXiCases[0, 0] = -1;
                EtaXiCases[0, 1] = -1;

                EtaXiCases[1, 0] = 0;
                EtaXiCases[1, 1] = -1;

                EtaXiCases[2, 0] = -1;
                EtaXiCases[2, 1] = 0;
            }
            else if (U_rot > 0 && V_rot < 0)
            {
                EtaXiCases[0, 0] = 1;
                EtaXiCases[0, 1] = -1;

                EtaXiCases[1, 0] = 0;
                EtaXiCases[1, 1] = -1;

                EtaXiCases[2, 0] = 1;
                EtaXiCases[2, 1] = 0;
            }
            else if (U_rot < 0 && V_rot > 0)
            {
                EtaXiCases[0, 0] = -1;
                EtaXiCases[0, 1] = 1;

                EtaXiCases[1, 0] = 0;
                EtaXiCases[1, 1] = 1;

                EtaXiCases[2, 0] = -1;
                EtaXiCases[2, 1] = 0;
            }
            return EtaXiCases;
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
