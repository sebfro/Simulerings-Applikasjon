using Microsoft.Research.Science.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpagettiMetoden
{
    class CalcDistance_BetweenTwoLonLatCoordinates
    {
        public BlockingCollection<PositionData> PositionDataList { get; set;}
        public ExtractDataFromEtaAndXi ExtractDataFromEtaAndXi { get; set; }

        public int Increment { get; set; }
        public int Increment2 { get; set; }

        public object syncObject = new object();

        public double getLatOrLon(int eta, int xi, Array LatOrLonArray)
        {
            return ExtractDataFromEtaAndXi.GetLatOrLon(eta, xi, LatOrLonArray);
        }

        public CalcDistance_BetweenTwoLonLatCoordinates(int inc, int inc2, int depthDelta)
        {
            DataSet ds = DataSet.Open(GlobalVariables.pathToNcHeatMaps);
            ExtractDataFromEtaAndXi = new ExtractDataFromEtaAndXi(
                ds["h"].GetData(), 
                DataSet.Open(GlobalVariables.pathToNcHeatMapFolder + "NK800_Z.nc")["Z"].GetData(),
                ds["mask_rho"].GetData(),
                depthDelta
                );
            Increment = inc;
            Increment2 = inc2;
        }

        public void SetDepthDelta(int DepthDelta)
        {
            ExtractDataFromEtaAndXi.DepthDelta = DepthDelta;
        }

        //Gir i km
        public static double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
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
        

        public EtaXi[] CalculatePossibleEtaXi(int eta, int xi)
        {
            //int increment = GlobalVariables.increment;
            //int increment2 = GlobalVariables.increment2;

            EtaXi[] etaXis = new EtaXi[17] {
                GenerateEtaXi(eta+Increment, xi-Increment, eta, xi),
                GenerateEtaXi(eta+Increment, xi, eta, xi),
                GenerateEtaXi(eta+Increment, xi+Increment, eta, xi),
                GenerateEtaXi(eta, xi-Increment, eta, xi),
                GenerateEtaXi(eta-Increment, xi-Increment, eta, xi),
                GenerateEtaXi(eta-Increment, xi, eta, xi),
                GenerateEtaXi(eta-Increment, xi+Increment, eta, xi),
                GenerateEtaXi(eta, xi+Increment, eta, xi),
                GenerateEtaXi(eta, xi, eta, xi),
                GenerateEtaXi(eta+Increment2, xi-Increment2, eta, xi),
                GenerateEtaXi(eta+Increment2, xi, eta, xi),
                GenerateEtaXi(eta+Increment2, xi+Increment2, eta, xi),
                GenerateEtaXi(eta, xi-Increment2, eta, xi),
                GenerateEtaXi(eta-Increment2, xi-Increment2, eta, xi),
                GenerateEtaXi(eta-Increment2, xi, eta, xi),
                GenerateEtaXi(eta-Increment2, xi+Increment2, eta, xi),
                GenerateEtaXi(eta, xi+Increment2, eta, xi)};

            return etaXis.Where(etaXi => etaXi.Valid).ToArray();;

        }

        public BlockingCollection<PositionData> FindValidPositions(EtaXi[] etaXis, Array latDataArray, Array lonDataArray, TagData tagData, CallPython callPython, double tempDelta)
        {
            //CalculateXiAndEta calculateXiAndEta = new CalculateXiAndEta();
            PositionDataList = new BlockingCollection<PositionData>();
            //extractDataFromEtaAndXi = new ExtractDataFromEtaAndXi();
            //PositionData positionData = new PositionData();
            double depth = 0.0;
            double temp = 0.0;
            double lat = 0.0;
            double lon = 0.0;
            DepthData depthData;


            for (int i = 0; i < etaXis.Length; i++)
            {
                lock (syncObject)
                {
                    depth = ExtractDataFromEtaAndXi.GetDepth(etaXis[i].Eta_rho, etaXis[i].Xi_rho);
                    depthData = ExtractDataFromEtaAndXi.GetS_rhoValues(etaXis[i].Eta_rho, etaXis[i].Xi_rho, tagData.depth);
                }
                
                if(depthData.Valid && (depth - (-tagData.depth)) > 0)
                {
                    lock (syncObject)
                    {
                        temp = callPython.GetTemp(depthData.Z_rho, etaXis[i].Eta_rho, etaXis[i].Xi_rho);
                        //callPython.getTempFromNorKyst(day, depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho);
                    }


                    if (Math.Abs(temp - tagData.temp) < tempDelta)
                    {

                        lock (syncObject)
                        {
                            lat = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, latDataArray);
                            lon = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, lonDataArray);
                        }
                        
                        PositionDataList.Add(new PositionData(lat, lon, depth, temp, tagData.depth, tagData.temp, etaXis[i].Eta_rho, etaXis[i].Xi_rho));
                    }
                }
            }

            return PositionDataList;
        }

        public EtaXi GenerateEtaXi(int eta, int xi, int org_eta, int org_xi)
        {
            bool valid = eta <= GlobalVariables.eta_rho_size && eta >= 0 && xi <= GlobalVariables.xi_rho_size && xi >= 0;
            if (valid)
            {
                int etaDiff = org_eta - eta;
                int xiDiff = org_xi - xi;
                lock (syncObject)
                {
                    if (etaDiff > 0 && xiDiff == 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta + i, xi))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff == 0 && xiDiff > 0)
                    {
                        for (int i = 1; i < xiDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta, xi + i))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff < 0 && xiDiff == 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta - i, xi))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff == 0 && xiDiff < 0)
                    {
                        for (int i = 1; i < xiDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta, xi - i))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff > 0 && xiDiff > 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta + i, xi + i))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff < 0 && xiDiff < 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta - i, xi - i))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff < 0 && xiDiff > 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta - i, xi + i))
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (etaDiff > 0 && xiDiff < 0)
                    {
                        for (int i = 1; i < etaDiff; i++)
                        {
                            if (ExtractDataFromEtaAndXi.IsOnLand(eta + i, xi - i))
                            {
                                valid = false;
                            }
                        }
                    }
                }
            }
            return new EtaXi(eta, xi, valid);
        }
        
    }

    class EtaXi
    {
        public int Eta_rho { get; set; }
        public int Xi_rho { get; set; }
        public bool Valid { get; set; }

        public EtaXi(int eta, int xi, bool valid)
        {
            
            Eta_rho = eta;
            Xi_rho = xi;
            Valid = valid;
        }
    }
}
