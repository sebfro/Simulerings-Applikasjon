using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpagettiMetoden
{
    class CalcDistance_BetweenTwoLonLatCoordinates
    {
        //Gir i km
        public double getDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
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

        public double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
        //speed er i km og time er timer. OBS!! Deprecated (kanskje)
        public LatLon[] calculatePossibleLatLon(double lat, double lon, double speed, int time)
        {

            var distance = (speed * time); //Gir km i timen, derfor deler vi på 6 for å få for hvert tiende minutt.
            int radius = 6371;            //Earth radius in Km

            int size = 8;
            LatLon[] latLonArray = new LatLon[size];
            int bearing = 0;
            for (int i = 0; i < size; i++)
            {
                var lat2 = Math.Asin(Math.Sin(Math.PI / 180 * lat) * Math.Cos(distance / radius) +
                                     Math.Cos(Math.PI / 180 * lat) * Math.Sin(distance / radius) * Math.Cos(Math.PI / 180 * bearing));
                var lon2 = Math.PI / 180 * lon + Math.Atan2(
                               Math.Sin(Math.PI / 180 * bearing) * Math.Sin(distance / radius) * Math.Cos(Math.PI / 180 * lat),
                               Math.Cos(distance / radius) - Math.Sin(Math.PI / 180 * lat) * Math.Sin(lat2));

                latLonArray[i] = new LatLon(180 / Math.PI * lat2, 180 / Math.PI * lon2);
                bearing += 45;
            }
            return latLonArray;
        }

        public EtaXi[] calculatePossibleEtaXi(int eta, int xi)
        {
            int increment = 5;
            int increment2 = 2;

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

            return etaXis.Where(etaXi => etaXi.valid).ToArray();

        }

        public List<PositionData> FindValidLatLons(EtaXi[] etaXis, Array latDataArray, Array lonDataArray, TagData tagData, Array depthArray, Array Z_Array)
        {
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            List<PositionData> positionDataList = new List<PositionData>();
            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            CallPython callPython = new CallPython();
            PositionData positionData = new PositionData();
            
            for (int i = 0; i < etaXis.Length; i++)
            {
                //PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(latDataArray, lonDataArray, latLon[i].lat,
                //    latLon[i].lon);
                positionData.depth = extractDataFromEtaAndXi.getDepth(etaXis[i].eta_rho, etaXis[i].xi_rho, depthArray);
                DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(etaXis[i].eta_rho, etaXis[i].xi_rho, tagData.depth, Z_Array);
                if(depthData.valid && (positionData.depth - (-tagData.depth)) > 0)
                {
                    //Console.WriteLine("s_rho: " + depthData.z_rho);

                    //var watch = Stopwatch.StartNew();

                    positionData.temp = callPython.getTempFromOceanAvg(int.Parse(tagData.day), depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho, tagData.year, tagData.month);

                    /*
                     * watch.Stop();
                    double elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine("Hvor lang tid tok det å hente temp med python: " + elapsedMs);
                     */

                    //positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, tempArray);

                    //Console.WriteLine("position data depth: " + positionData.depth + " , tagdata depth: " + tagData.depth + " , position data temp: " + positionData.temp + " , tag data temp: " + tagData.temp);

                    if (Math.Abs(positionData.temp - tagData.temp) < 3.5)
                    {
                        //Console.WriteLine("Inni for-løkken sin if, Noe ble valid");
                        positionData.eta_rho = etaXis[i].eta_rho;
                        positionData.xi_rho = etaXis[i].xi_rho;
                        positionData.lat = extractDataFromEtaAndXi.getLatorLon(positionData.eta_rho, positionData.xi_rho, latDataArray);
                        positionData.lon = extractDataFromEtaAndXi.getLatorLon(positionData.eta_rho, positionData.xi_rho, lonDataArray);
                        positionDataList.Add(positionData);
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

    class LatLon
    {
        public double lat { get; set; }
        public double lon { get; set; }

        public LatLon(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
    }
}
