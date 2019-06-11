using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Translator;
using Cudafy.Host;
using Microsoft.Research.Science.Data;
using System.Diagnostics;

namespace SimuleringsApplikasjonen
{
    class SimpleGPUAcceleration
    {
        private int N = 1000;
        public static void startUp()
        {
            int norkyst_eta_rho = 902;
            int norkyst_xi_rho = 2602;
            int barents_eta_rho = 580;
            int barents_xi_rho = 1202;

            DataSet norkystDs = DataSet.Open(@"D:\NCdata\Nye norkyst\norkyst_800m_avg.nc");
            Array norkystLatArray = norkystDs["lat_rho"].GetData();
            double[,] norkystLatTable = new double[norkyst_eta_rho, norkyst_xi_rho];
            Array norkystLonArray = norkystDs["lon_rho"].GetData();
            double[,] norkystLonTable = new double[norkyst_eta_rho, norkyst_xi_rho];
            for (int etaInc = 0; etaInc < norkyst_eta_rho; etaInc++)
            {
                for (int xiInc = 0; xiInc < norkyst_xi_rho; xiInc++)
                {
                    norkystLatTable[etaInc, xiInc] = (double)norkystLatArray.GetValue(etaInc, xiInc);
                    norkystLonTable[etaInc, xiInc] = (double)norkystLonArray.GetValue(etaInc, xiInc);
                }
            }

            DataSet barentsDs = DataSet.Open(@"D:\NCdata\ocean_avg_LatAndLonRho.nc");
            Array barentsLatArray = barentsDs["lat_rho"].GetData();
            double[,] barentsLatTable = new double[barents_eta_rho, barents_xi_rho];
            Array barentsLonArray = barentsDs["lon_rho"].GetData();
            double[,] barentsLonTable = new double[barents_eta_rho, barents_xi_rho];
            for (int etaInc = 0; etaInc < barents_eta_rho; etaInc++)
            {
                for (int xiInc = 0; xiInc < barents_xi_rho; xiInc++)
                {
                    barentsLatTable[etaInc, xiInc] = (double)barentsLatArray.GetValue(etaInc, xiInc);
                    barentsLonTable[etaInc, xiInc] = (double)barentsLonArray.GetValue(etaInc, xiInc);
                }
            }

            CudafyModes.Target = eGPUType.OpenCL;
            CudafyTranslator.Language = eLanguage.OpenCL;
            CudafyModule km = CudafyTranslator.Cudafy();
            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            //Disse er utenfor den doble for løkken for å unngå for høy vram og ram bruk
            double[,] dev_norkystLatTable = gpu.CopyToDevice(norkystLatTable);
            double[,] dev_norkystLonTable = gpu.CopyToDevice(norkystLonTable);
            double[,] dev_barentsLatTable = gpu.CopyToDevice(barentsLatTable);
            double[,] dev_barentsLonTable = gpu.CopyToDevice(barentsLonTable);


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int eta = 0; eta < 3; eta++)
            {
            
                for (int xi = 0; xi < norkyst_xi_rho; xi++)
                {

                    //Dette gjør mer eller mindre det samme som den under, men her kan jeg sette verdiene som sendes inn. Derfor bruker jeg denne metoden
                    //Poenget er at jeg har en eta og xi for f.eks. norkyst som jeg vil konvertere til barentshav koordinater. Da sendes de inn og deretter
                    //kommer de ut som barentshav koordinater
                    int[] tmpEta = { eta };
                    int[] dev_Eta = gpu.CopyToDevice(tmpEta);
                    int[] tmpXi = { xi };
                    int[] dev_Xi = gpu.CopyToDevice(tmpXi);

                    int[] dev_returnEta = gpu.Allocate(new int[1]);
                    int[] dev_returnXi = gpu.Allocate(new int[1]);

                    //Allokerer minne for to variabler på GPU, så vidt jeg forstår så må de være []. Jeg vil bare ha en varaibel så det er bare et element i dem
                    //int[] dev_eta = gpu.Allocate<int>(1);
                    //int[] dev_xi = gpu.Allocate<int>(1);
                    //Kjører metoden på gpu, den første parameteren i launch er hvor mange parallele instanser av metoden som kjøres.
                    //gpu.Launch(1,10).ReturnTest(dev_Eta, dev_Xi, dev_norkystLatTable, dev_norkystLonTable, dev_barentsLatTable, dev_barentsLonTable);
                    //Henter de to variablene fra gpu etter metoden har fullført
                    gpu.CopyFromDevice(dev_Eta, out int newEta);
                    gpu.CopyFromDevice(dev_Xi, out int newXi);
                    //double[,] result = new double[10,10];
                    /*
                    Console.WriteLine("Norkyst:");
                    Console.WriteLine("Eta: {0}, Xi: {1}", eta, xi);
                    Console.WriteLine("Barents Havet:");*/
                    Console.WriteLine("Eta: {0}, Xi: {1}", newEta, newXi);
                    
                }
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("GPU RunTime " + elapsedTime);
            Stopwatch stopWatch1 = new Stopwatch();
            stopWatch1.Start();
            for (int eta = 0; eta < 3; eta++)
            {
                for (int xi = 0; xi < 10; xi++)
                {
                    int[] etaXi = StdTest(eta, xi, norkystLatTable, norkystLonTable, barentsLatTable, barentsLonTable);
                    Console.WriteLine("Eta: {0}, Xi: {1}", etaXi[0], etaXi[1]);
                }
            }
            stopWatch1.Stop();
            // Get the elapsed time as a TimeSpan value.
            ts = stopWatch1.Elapsed;

            // Format and display the TimeSpan value.
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("CPU RunTime " + elapsedTime);

            
        }

        [Cudafy]
        public static void ReturnTest(GThread thread, int[] eta, int[] xi, double[,] norkystLat, double[,] norkystLon, double[,] barentsLat, double[,] barentsLon)
        {
            double[] cache = thread.AllocateShared<double>("cache", 10);

            //int tid = thread.blockIdx.x;
            int norkyst_eta_rho = 902;
            int norkyst_xi_rho = 2602;
            int barents_eta_rho = 580;
            int barents_xi_rho = 1202;

            int currEta = eta[0];
            int currXi = xi[0];
            int tmpEta = 0;
            int tmpXi = 0;
            double mindelta = 1000;

            double lat = norkystLat[currEta, currXi];
            double lon = norkystLon[currEta, currXi];

            for (int etaInc = 0; etaInc < barents_eta_rho; etaInc++){
                //int etaInc = tid;
                //if(tid < barents_eta_rho) {
                //thread.SyncThreads();
                for (int xiInc = 0; xiInc < barents_xi_rho; xiInc++)
                {
                    double latDiff = (barentsLat[etaInc, xiInc] - lat);
                    double lonDiff = (barentsLon[etaInc, xiInc] - lon);
                    
                    if (latDiff < 0)
                    {
                        latDiff = latDiff * (-1);
                    }
                    
                    if (lonDiff < 0)
                    {
                        lonDiff = lonDiff * (-1);
                    }
                    
                    double newDelta = latDiff + lonDiff;
                    if(newDelta < mindelta)
                    {
                        thread.SyncThreads();
                        mindelta = newDelta;
                        tmpEta = etaInc;
                        tmpXi = xiInc;
                    }
                }
            }
            eta[0] = tmpEta;
            xi[0] = tmpXi;
        }

        public static int[] StdTest(int eta, int xi, double[,] norkystLat, double[,] norkystLon, double[,] barentsLat, double[,] barentsLon)
        {
            int norkyst_eta_rho = 902;
            int norkyst_xi_rho = 2602;
            int barents_eta_rho = 580;
            int barents_xi_rho = 1202;

            int currEta = eta;
            int currXi = xi;
            int tmpEta = 0;
            int tmpXi = 0;
            double mindelta = 1000;

            double lat = norkystLat[currEta, currXi];
            double lon = norkystLon[currEta, currXi];

            //for (int etaInc = 0; etaInc < barents_eta_rho; etaInc++)
            Parallel.For(0, barents_eta_rho, etaInc =>
             {
                 for (int xiInc = 0; xiInc < barents_xi_rho; xiInc++)
                 {
                     double latDiff = (barentsLat[etaInc, xiInc] - lat);
                     double lonDiff = (barentsLon[etaInc, xiInc] - lon);

                     if (latDiff < 0)
                     {
                         latDiff = latDiff * (-1);
                     }

                     if (lonDiff < 0)
                     {
                         lonDiff = lonDiff * (-1);
                     }

                     double newDelta = latDiff + lonDiff;
                     if (newDelta < mindelta)
                     {
                         mindelta = newDelta;
                         tmpEta = etaInc;
                         tmpXi = xiInc;
                     }
                 }
             });
            return new int[2] { tmpEta, tmpXi };

        }
    }
}