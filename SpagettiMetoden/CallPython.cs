using MathNet.Numerics;
using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SpagettiMetoden
{
    class CallPython
    {
        public static int add_offset = 10;
        public static double scale_factor = 0.001;
        public Dictionary<int, Array> tempDictionary = new Dictionary<int, Array>();
        public int day;

        public Array tempArray;
        public CallPython(int dayIncrement)
        {
            //tempArray = DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + GlobalVariables.day + ".nc")["temp"].GetData();
            //tempArray = DataSet.Open(@"C:\NCData\VarmeModell\TestFiles\ocean_time" + GlobalVariables.day + ".nc")["temp"].GetData();
            day = GlobalVariables.day;
            for(int i = 29; i <= 225; i += dayIncrement)
            {
                //Console.WriteLine("Index: {0}", i);
                tempDictionary.Add(i, DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + i + ".nc")["temp"].GetData());
            }

            Console.WriteLine("Success: Alle heat maps have been loaded");
            updateTempArray(GlobalVariables.day);
        }

        public void updateDay(int day)
        {
            this.day = day;
        }
        //Setter tempArray til den korrekte dagen for neste iterasjon, samme som før.
        //Men den hentes ut av en dictionary i rammen istedenfor fra hdd/ssd
        public void updateTempArray(int day)
        {
            //out tempArray setter variablen tempArray til det vi får ut av TryGetValue
            tempDictionary.TryGetValue(day, out tempArray);
            //tempArray = DataSet.Open(GlobalVariables.pathToOceanTimeNetCDF + day + ".nc")["temp"].GetData();
        }

        /// <summary>
        /// scale_factor og add_offset brukes her for å omkonvertere en int16
        /// tilbake til en double verdi slik at vi kan bruke den
        /// </summary>
        /// <param name="z_rho"></param>
        /// <param name="eta_rho"></param>
        /// <param name="xi_rho"></param>
        /// <returns>temperatur fra en en array4D av temperatur</returns>
        public double getTemp(int z_rho, int eta_rho, int xi_rho)
        {
            eta_rho -= 1;
            xi_rho -= 1;

            double tempValue = (short)tempArray.GetValue(z_rho, eta_rho, xi_rho);
            return (tempValue * scale_factor) + add_offset;
        }
        

    }
}
