using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Science.Data;

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
        //speed er i km og time er timer
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

        public List<PositionData> FindValidLatLons(LatLon[] latLon, Array latDataArray, Array lonDataArray, TagData tagData, Array depthArray, Array tempArray, Array Z_Array)
        {
            CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            List<PositionData> positionDataList = new List<PositionData>();
            ExtractDataFromEtaAndXi extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            
            for (int i = 0; i < latLon.Length; i++)
            {
                PositionData positionData = calculateXiAndEta.GeneratePositionDataArrayList(latDataArray, lonDataArray, latLon[i].lat,
                    latLon[i].lon);
                positionData.depth = extractDataFromEtaAndXi.getDepth(positionData.eta_rho, positionData.xi_rho, depthArray);
                DepthData depthData = extractDataFromEtaAndXi.getS_rhoValues(positionData.eta_rho, positionData.xi_rho, tagData.depth, Z_Array);
                if(depthData.valid)
                {
                    //Console.WriteLine("s_rho: " + depthData.z_rho);
                    positionData.temp = extractDataFromEtaAndXi.getTemp(0, depthData.z_rho, positionData.eta_rho, positionData.xi_rho, tempArray);

                    //Console.WriteLine("position data depth: " + positionData.depth + " , tagdata depth: " + tagData.depth + " , position data temp: " + positionData.temp + " , tag data temp: " + tagData.temp);

                    if ((positionData.depth - (-tagData.depth)) > 0 && Math.Abs(positionData.temp - tagData.temp) < 5)
                    {
                        //Console.WriteLine("Inni for-løkken sin if, Noe ble valid");
                        positionDataList.Add(positionData);
                    }
                }
            }

            return positionDataList;
        }
        
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
