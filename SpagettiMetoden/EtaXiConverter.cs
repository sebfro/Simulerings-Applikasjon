using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuleringsApplikasjonen
{
    class EtaXiConverter
    {
        //Lat and lon for norkyst and barents sea
        private Array NorkystLatArray { get; set; }
        private Array NorkystLonArray { get; set; }
        private Array BarentsLatArray { get; set; }
        private Array BarentsLonArray { get; set; }

        //The indexes for norkyst and barents sea
        //Used to convert norkyst to barents sea and vice versa
        private Array BarentsToNorkystEta { get; set; }
        private Array BarentsToNorkystXi { get; set; }
        private Array NorkystTobarentsEta { get; set; }
        private Array NorkystTobarentsXi { get; set; }

        public EtaXiConverter()
        {
            DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMapnorkyst);
            NorkystLatArray = ds["lat_rho"].GetData();
            NorkystLonArray = ds["lon_rho"].GetData();

            ds = DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "mndmean_avg_200810.nc");
            BarentsLatArray = ds["lat_rho"].GetData();
            BarentsLonArray = ds["lon_rho"].GetData();

            ds = DataSet.Open(@"C:\NCdata\barentsSeaMappedToNorkyst.nc");
            BarentsToNorkystEta = ds["eta_rho"].GetData();
            BarentsToNorkystXi = ds["xi_rho"].GetData();

            ds = DataSet.Open(@"C:\NCdata\norkystMappedToBarentsSea.nc");
            NorkystTobarentsEta = ds["eta_rho"].GetData();
            NorkystTobarentsXi = ds["xi_rho"].GetData();
        }

        //Hvis use_norkyst er true så antar metoden at du skal bytte til norkyst fra barents havet. Omvendt hvis false.
        //Hvis use_norkyst = false må eta < 902 og xi < 2602. Begger må være større enn 0
        //Hvis use_norkyst = true må eta < 580 og xi < 1202. Begger må være større enn 0
        public EtaXi ConvertNorkystOrBarents(int eta, int xi, bool use_norkyst)
        {
            int norkyst_eta_rho = 902;
            int norkyst_xi_rho = 2602;
            int barents_eta_rho = 580;
            int barents_xi_rho = 1202;


            if ((!use_norkyst && (0 > eta || eta >= norkyst_eta_rho || 0 > xi || xi >= norkyst_xi_rho)) || use_norkyst && (0 > eta || eta >= barents_eta_rho || 0 > xi || xi >= barents_xi_rho))
            {
                throw new ArgumentOutOfRangeException();
            }
            int returnEta;
            int returnXi;
            double norkystLat;
            double norkystLon;
            double barentsLat;
            double barentsLon;

            /*
             DataSet barentsToNorkyst = DataSet.Open(@"C:\NCdata\barentsSeaMappedToNorkyst.nc");
             Array barentsToNorkystEta = barentsToNorkyst["eta_rho"].GetData();
             Array barentsToNorkystXi = barentsToNorkyst["xi_rho"].GetData();

             DataSet norkystToBarents = DataSet.Open(@"C:\NCdata\norkystMappedToBarentsSea.nc");
             Array NorkystTobarentsEta = norkystToBarents["eta_rho"].GetData();
             Array NorkystTobarentsXi = norkystToBarents["xi_rho"].GetData();

             DataSet norkystDs = DataSet.Open(@"D:\NCdata\VarmeModell\norkyst_800m_avg.nc");
             Array norkystLatArray = norkystDs["lat_rho"].GetData();
             Array norkystLonArray = norkystDs["lon_rho"].GetData();

             DataSet barentsDs = DataSet.Open(@"D:\NCdata\VarmeModell\mndmean_avg_200810.nc");
             Array barentsLatArray = barentsDs["lat_rho"].GetData();
             Array barentsLonArray = barentsDs["lon_rho"].GetData();
             */

            if (use_norkyst)
            {
                barentsLat = (double)BarentsLatArray.GetValue(eta, xi);
                barentsLon = (double)BarentsLonArray.GetValue(eta, xi);

                returnEta = (int)BarentsToNorkystEta.GetValue(eta, xi);
                returnXi = (int)BarentsToNorkystXi.GetValue(eta, xi);

                norkystLat = (double)NorkystLatArray.GetValue(returnEta, returnXi);
                norkystLon = (double)NorkystLonArray.GetValue(returnEta, returnXi);


            }
            else
            {
                norkystLat = (double)NorkystLatArray.GetValue(eta, xi);
                norkystLon = (double)NorkystLonArray.GetValue(eta, xi);

                returnEta = (int)NorkystTobarentsEta.GetValue(eta, xi);
                returnXi = (int)NorkystTobarentsXi.GetValue(eta, xi);

                barentsLat = (double)BarentsLatArray.GetValue(returnEta, returnXi);
                barentsLon = (double)BarentsLonArray.GetValue(returnEta, returnXi);
            }

            /*
            Console.WriteLine("Norkyst Lat: {0}, Lon: {1}", norkystLat, norkystLon);
            Console.WriteLine("Barents Lat: {0}, Lon: {1}", barentsLat, barentsLon);

            Console.WriteLine("Eta: {0}, xi: {1}", eta, xi);
            */
            return new EtaXi(returnEta, returnXi, (Math.Abs(norkystLat - barentsLat) + Math.Abs(norkystLon - barentsLon)) < 1);

        }
    }
}
