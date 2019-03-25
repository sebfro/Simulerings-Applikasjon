using Microsoft.Research.Science.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.CSharp;
using SpagettiMetoden.Info_Containers;

namespace SpagettiMetoden
{
    class CalculateCoordinates
    {
        public BlockingCollection<PositionData> PositionDataList { get; set;}
        public ExtractDataFromEtaAndXi ExtractDataFromEtaAndXi { get; set; }

        public double Increment { get; set; }
        public int Iterations { get; set; }

        public double DayInc { get; set; }
        
        public object syncObject = new object();

        public static readonly int[,] EtaXiCases = new int[,]
        {
            {1,-1},{1, 0},{1,1},{0, -1}, {-1, -1}, {-1, 0}, {-1, 1},{0, 1}
        };

        public double GetLatOrLon(int eta, int xi, Array LatOrLonArray)
        {
            return ExtractDataFromEtaAndXi.GetLatOrLon(eta, xi, LatOrLonArray);
        }

        public CalculateCoordinates(double inc, int depthDelta, double dayInc, int iterations)
        {
            ExtractDataFromEtaAndXi = new ExtractDataFromEtaAndXi(depthDelta);

            Iterations = iterations;
            DayInc = dayInc;
            Increment = inc;
        }

        public void SetDepthDelta(int DepthDelta)
        {
            ExtractDataFromEtaAndXi.DepthDelta = DepthDelta;
        }

        //Gir i km
        public static double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2rad(lon2 - lon1);
            var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
        
        public int GenerateIncrement(float min, float max, float divideBy)
        {
            //Har fjernet 0.71, må finne ut om vi skal ha en annen verdi der eller ikke.
            return (int)((((1 * Increment * ThreadSafeRandom.RandomSpeed(min, max)) * 3.6) * (DayInc * 24)) / divideBy);
        }

        public EtaXi[] CalculatePossibleEtaXi(int eta, int xi, bool lowerSpeed, double depth, bool use_norkyst)
        {

            EtaXi[] EtaXis = new EtaXi[Iterations+1];
            float max = lowerSpeed ? 0.4f : 1f;
            float min = lowerSpeed ? 0.01f : 0.4f;
            float divideBy = use_norkyst ? 0.8f : 4f;
            int increment = GenerateIncrement(min, max, divideBy);
            int counter = 0;
            for (int i = 0; i < Iterations; i++)
            {
                    EtaXis[i] = (GenerateEtaXi(eta + (EtaXiCases[counter,0] * increment), xi + (EtaXiCases[counter,1] * increment), eta, xi, use_norkyst));
                if (counter == 7)
                {
                    counter = 0;
                    if (max > 0.5)
                    {
                        max -= 0.1f;
                    }
                    increment = GenerateIncrement(min, max, divideBy);
                }
                else
                {
                    counter++;
                }
            }

            EtaXis[Iterations] = new EtaXi(eta, xi, true);

            return EtaXis.Where(etaXi => etaXi.Valid).ToArray();

        }

        public BlockingCollection<PositionData> FindValidPositions(EtaXi[] etaXis, Array latNorkystArray, Array lonNorkystArray, Array latBarentsArray, Array lonBarentsArray, TagData tagData, TempContainer tempContainer, double tempDelta, bool use_norkyst)
        {
            PositionDataList = new BlockingCollection<PositionData>();
            double depth = 0.0;
            double temp = 10;
            double lat = 0.0;
            double lon = 0.0;
            List<DepthData> potentialDepthArray = new List<DepthData>();
            DepthData depthData;
            for (int i = 0; i < etaXis.Length; i++)
            {
                lock (syncObject)
                {
                    depth = ExtractDataFromEtaAndXi.GetDepth(etaXis[i].Eta_rho, etaXis[i].Xi_rho, use_norkyst);
                    depthData = ExtractDataFromEtaAndXi.GetS_rhoValues(etaXis[i].Eta_rho, etaXis[i].Xi_rho, tagData.Depth, use_norkyst);
                    //potentialDepthArray = ExtractDataFromEtaAndXi.GetS_rhoValues(etaXis[i].Eta_rho, etaXis[i].Xi_rho, tagData.Depth, use_norkyst);
                }
                double newTemp = 0.0;
                /*
                List<int> depthList = new List<int>();
                depthList.Add(depthData.Z_rho);
                if (depthData.Z_rho > 0)
                {
                    depthList.Add(depthData.Z_rho - 1);
                }
                int z_rho_max = use_norkyst ? GlobalVariables.Z_rho_size_norkyst : GlobalVariables.Z_rho_size_ocean_avg;
                if (depthData.Z_rho < z_rho_max - 1)
                {
                    depthList.Add(depthData.Z_rho + 1);
                }*/
                if ((depth - (-tagData.Depth)) > 0)
                {

                    
                    lock (syncObject)
                    {
                        temp = tempContainer.GetTemp(depthData.Z_rho, etaXis[i].Eta_rho, etaXis[i].Xi_rho, use_norkyst);
                    }
                    

                    //tempContainer.getTempFromNorKyst(day, depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho);



                    /*
                    foreach (DepthData dData in potentialDepthArray)
                    {
                        lock (syncObject)
                        {
                            newTemp = tempContainer.GetTemp(dData.Z_rho, etaXis[i].Eta_rho, etaXis[i].Xi_rho, use_norkyst);
                            if(Math.Abs(newTemp - tagData.Temp) < tempDelta && Math.Abs(newTemp - tagData.Temp) < Math.Abs(temp - tagData.Temp))
                            {
                                temp = newTemp;
                            }
                            //tempContainer.getTempFromNorKyst(day, depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho);
                        }
                    }
                    */



                    if (Math.Abs(temp - tagData.Temp) < tempDelta)
                    {

                        lock (syncObject)
                        {
                            if (use_norkyst)
                            {
                                lat = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, latNorkystArray);
                                lon = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, lonNorkystArray);
                            } else
                            {
                                lat = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, latBarentsArray);
                                lon = ExtractDataFromEtaAndXi.GetLatOrLon(etaXis[i].Eta_rho, etaXis[i].Xi_rho, lonBarentsArray);
                            }
                        }
                        
                        PositionDataList.Add(new PositionData(lat, lon, depth, temp, tagData.Depth, tagData.Temp, etaXis[i].Eta_rho, etaXis[i].Xi_rho, etaXis[i].ExtraWeight));
                    }
                }
            }

            return PositionDataList;
        }

        public int SetIncrement(int newCoord, int oldCoord)
        {
            if (newCoord > oldCoord)
            {
                return 1;
            } else if (newCoord < oldCoord)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public EtaXi GenerateEtaXi(int eta, int xi, int org_eta, int org_xi, bool use_norkyst)
        {
            int eta_rho_size;
            int xi_rho_size;
            if (use_norkyst)
            {
                eta_rho_size = GlobalVariables.eta_rho_size_norkyst;
                xi_rho_size = GlobalVariables.xi_rho_size_norkyst;
            }
            else
            {
                eta_rho_size = GlobalVariables.eta_rho_size_ocean_avg;
                xi_rho_size = GlobalVariables.xi_rho_size_ocean_avg;
            }
            bool valid = eta < eta_rho_size && eta >= 0 && xi < xi_rho_size && xi >= 0;
            if (valid)
            {
                int etaInc = SetIncrement(eta, org_eta);
                int xiInc = SetIncrement(xi, org_xi);
                int iterasion = Math.Abs(eta - org_eta);
                iterasion = iterasion == 0 ? Math.Abs(xi - org_xi) : iterasion;
                for (int i = 0; i < iterasion && valid; i++)
                {
                    if (ExtractDataFromEtaAndXi.IsOnLand(org_eta + (i * etaInc), org_xi + (i * xiInc), use_norkyst))
                    {
                        valid = false;
                    }
                }
            }
            return new EtaXi(eta, xi, valid);
        }
        
    }

    

    
}
