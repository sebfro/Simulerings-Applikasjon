using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpagettiMetoden
{
    class CalcDistance_BetweenTwoLonLatCoordinates
    {
        //Gir i km
        public static double getDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
        

        public static EtaXi[] calculatePossibleEtaXi(int eta, int xi)
        {
            int increment = 40;
            int increment2 = 20;

            EtaXi[] etaXis = new EtaXi[17] {
                new EtaXi(eta+increment, xi-increment),
                new EtaXi(eta+increment, xi),
                new EtaXi(eta+increment, xi+increment),
                new EtaXi(eta, xi-increment),
                new EtaXi(eta-increment, xi-increment),
                new EtaXi(eta-increment, xi),
                new EtaXi(eta-increment, xi+increment),
                new EtaXi(eta, xi+increment),
                new EtaXi(eta, xi),
                new EtaXi(eta+increment2, xi-increment2),
                new EtaXi(eta+increment2, xi),
                new EtaXi(eta+increment2, xi+increment2),
                new EtaXi(eta, xi-increment2),
                new EtaXi(eta-increment2, xi-increment2),
                new EtaXi(eta-increment2, xi),
                new EtaXi(eta-increment2, xi+increment2),
                new EtaXi(eta, xi+increment2)};

            return etaXis.Where(etaXi => etaXi.valid).ToArray();;

        }

        public static BlockingCollection<PositionData> FindValidPositions(EtaXi[] etaXis, Array latDataArray, Array lonDataArray, TagData tagData, Array depthArray, Array Z_Array, int day)
        {
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            BlockingCollection<PositionData> positionDataList = new BlockingCollection<PositionData>();
            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            CallPython callPython = new CallPython();
            PositionData positionData = new PositionData();
            double depth = 0.0;
            double temp = 0.0;
            double lat = 0.0;
            double lon = 0.0;
            
            for (int i = 0; i < etaXis.Length; i++)
            {
                depth = extractDataFromEtaAndXi.getDepth(etaXis[i].eta_rho, etaXis[i].xi_rho, depthArray);
                DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(etaXis[i].eta_rho, etaXis[i].xi_rho, tagData.depth, Z_Array);
                if(depthData.valid && (depth - (-tagData.depth)) > 0)
                {
                    temp = callPython.getTempFromNorKyst(day, depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho);

                    if (Math.Abs(temp - tagData.temp) < 5)
                    {
               
                        lat = extractDataFromEtaAndXi.getLatOrLon(etaXis[i].eta_rho, etaXis[i].xi_rho, latDataArray);
                        lon = extractDataFromEtaAndXi.getLatOrLon(etaXis[i].eta_rho, etaXis[i].xi_rho, lonDataArray);
                        
                        positionDataList.Add(new PositionData(lat, lon, depth, temp, tagData.depth, tagData.temp, etaXis[i].eta_rho, etaXis[i].xi_rho));
                    }
                }
            }
            return positionDataList;
        }
        
    }

    class EtaXi
    {
        public int eta_rho { get; set; }
        public int xi_rho { get; set; }
        public bool valid { get; set; }

        public EtaXi(int eta, int xi)
        {
            valid = eta <= GlobalVariables.eta_rho_size && eta >= 0 && xi <= GlobalVariables.xi_rho_size && xi >= 0;
            eta_rho = eta;
            xi_rho = xi;
        }
        public EtaXi() { }
    }
}
