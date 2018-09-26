using MathNet.Numerics;
using Microsoft.Research.Science.Data;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SpagettiMetoden
{
    class CallPython
    {
        //Denne metoden kaller ett python script som henter ut temperatur fra ocean_avg og returnerer verdien som en double
        public double getTempFromOceanAvg(int ocean_time, int s_rho, int eta_rho, int xi_rho, string year, string month)
        {
            // full path of python interpreter 
            string python = @"C:\python\python.exe";

            // python app to call 
            string myPythonApp = @"C:\Users\a22001\Documents\GitHub\SDSLiteVS2017\SpagettiMetoden\getTempFromOcean_Avg.py";            

            // Create new process start info 
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);

            // make sure we can read the output from stdout 
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;

            // start python app with 3 arguments  
            // 1st arguments is pointer to itself,  
            // The other values are actual arguments we want to send (ocean_time to month)
            if(ocean_time > 29)
            {
                ocean_time = 29;
            } else
            {
                ocean_time--;
            }
            myProcessStartInfo.Arguments = myPythonApp + " " + ocean_time + " " + s_rho + " " + eta_rho + " " + xi_rho + " " + year + " " + month;

            Process myProcess = new Process();
            // assign start information to the process 
            myProcess.StartInfo = myProcessStartInfo;

            //Console.WriteLine("Calling Python script with arguments {0}, {1}, {2}, {3} and {4}", ocean_time, s_rho, eta_rho, xi_rho, month);
            // start the process 
            myProcess.Start();

            // Read the standard output of the app we called.  
            // in order to avoid deadlock we will read output first 
            // and then wait for process terminate: 
            StreamReader myStreamReader = myProcess.StandardOutput;
            string myString = myStreamReader.ReadLine();

            /*if you need to read multiple lines, you might use: 
                string myString = myStreamReader.ReadToEnd() */

            // wait exit signal from the app we called and then close it. 
            myProcess.WaitForExit();
            myProcess.Close();

            // write the output we got from python app 
            //Console.WriteLine("Value received from script: " + myString);
            return double.Parse(myString, CultureInfo.InvariantCulture);
        }

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
