using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class Converter
    {
        private static readonly int norkyst_eta_rho = 902;
        private static readonly int norkyst_xi_rho = 2602;
        private static readonly int barents_eta_rho = 580;
        private static readonly int barents_xi_rho = 1202;

        private DataSet dataSet;

        private Array BarentsToNorkystEta;
        private Array barentsToNorkystXi;

        //Paths to the NetCDF files
        private string barentsToNorkyst = @"C:\NCdata\barentsSeaMappedToNorkyst.nc";

        public Converter()
        {

        }

        public void ConvertNorkystOrBarents(int eta, int xi, bool use_norkyst)
        {
            /*
            int norkyst_eta_rho = 902;
            int norkyst_xi_rho = 2602;
            int barents_eta_rho = 580;
            int barents_xi_rho = 1202;
            */


            if ((!use_norkyst && (0 > eta || eta >= norkyst_eta_rho || 0 > xi || xi >= norkyst_xi_rho)) || use_norkyst && (0 > eta || eta >= barents_eta_rho || 0 > xi || xi >= barents_xi_rho))
            {
                throw new ArgumentOutOfRangeException();
            }

            DataSet barentsToNorkyst = DataSet.Open(@"C:\NCdata\barentsSeaMappedToNorkyst.nc");
            Array barentsToNorkystEta = barentsToNorkyst["eta_rho"].GetData();
            Array barentsToNorkystXi = barentsToNorkyst["xi_rho"].GetData();

            DataSet norkystToBarents = DataSet.Open(@"C:\NCdata\norkystMappedToBarentsSea.nc");
            Array NorkystTobarentsEta = norkystToBarents["eta_rho"].GetData();
            Array NorkystTobarentsXi = norkystToBarents["xi_rho"].GetData();

            DataSet norkystDs = DataSet.Open(@"D:\NCdata\VarmeModell\norkyst_800m_avg.nc");
            Array norkystLatArray = norkystDs["lat_rho"].GetData();
            Array norkystLonArray = norkystDs["lon_rho"].GetData();

            DataSet barentsDs = DataSet.Open(@"I:\mndmean_avg_200810.nc");
            Array barentsLatArray = barentsDs["lat_rho"].GetData();
            Array barentsLonArray = barentsDs["lon_rho"].GetData();


            int norkystEta = 0;
            int norkystXi = 0;
            int barentsEta = 0;
            int barentsXi = 0;

            if (use_norkyst)
            {
                barentsEta = eta;
                barentsXi = xi;
                norkystEta = (int)barentsToNorkystEta.GetValue(barentsEta, barentsXi);
                norkystXi = (int)barentsToNorkystXi.GetValue(barentsEta, barentsXi);
            }
            else
            {
                norkystEta = eta;
                norkystXi = xi;
                barentsEta = (int)NorkystTobarentsEta.GetValue(norkystEta, norkystXi);
                barentsXi = (int)NorkystTobarentsXi.GetValue(norkystEta, norkystXi);
            }


            Console.WriteLine("Norkyst Lat: {0}, Lon: {1}", (double)norkystLatArray.GetValue(norkystEta, norkystXi), (double)norkystLonArray.GetValue(norkystEta, norkystXi));
            Console.WriteLine("Barents Lat: {0}, Lon: {1}", (double)barentsLatArray.GetValue(barentsEta, barentsXi), (double)barentsLonArray.GetValue(barentsEta, barentsXi));

            Console.WriteLine("Norkyst eta: {0}, xi: {1}", norkystEta, norkystXi);
            Console.WriteLine("Barents eta: {0}, xi: {1}", barentsEta, barentsXi);
        }
    }
}
