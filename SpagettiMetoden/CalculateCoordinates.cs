using Microsoft.Research.Science.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.CSharp;

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
            //mndmean_avg_200810.nc
            ExtractDataFromEtaAndXi = new ExtractDataFromEtaAndXi(depthDelta);

            Iterations = iterations;
            DayInc = dayInc;
            Increment = inc;
            Console.WriteLine("Increment: {0}", Increment);
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
            return (int)(((Increment * ThreadSafeRandom.RandomSpeed(min, max) * 3.6) * (DayInc * 24)) / divideBy);
        }

        public EtaXi[] CalculatePossibleEtaXi(int eta, int xi, bool lowerSpeed, double depth, bool use_norkyst)
        {

            EtaXi[] EtaXis = new EtaXi[Iterations+1];
            float max = lowerSpeed ? 0.4f : 1f;
            float min = lowerSpeed ? 0.01f : 0.4f;
            float divideBy = use_norkyst ? 0.8f : 4f;
            int increment = GenerateIncrement(min, max, divideBy);
            DepthData depthData = ExtractDataFromEtaAndXi.GetS_rhoValues(eta, xi, depth, use_norkyst);
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
                    depth = ExtractDataFromEtaAndXi.GetDepth(etaXis[i].Eta_rho, etaXis[i].Xi_rho, use_norkyst);
                    depthData = ExtractDataFromEtaAndXi.GetS_rhoValues(etaXis[i].Eta_rho, etaXis[i].Xi_rho, tagData.Depth, use_norkyst);
                }
                
                if(depthData.Valid && (depth - (-tagData.Depth)) > 0)
                {
                    lock (syncObject)
                    {
                        temp = tempContainer.GetTemp(depthData.Z_rho, etaXis[i].Eta_rho, etaXis[i].Xi_rho, use_norkyst);
                        //tempContainer.getTempFromNorKyst(day, depthData.z_rho, etaXis[i].eta_rho, etaXis[i].xi_rho);
                    }


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
                eta_rho_size = GlobalVariables.eta_rho_size_ocean_time;
                xi_rho_size = GlobalVariables.xi_rho_size_ocean_time;
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

    class EtaXi
    {
        public int Eta_rho { get; set; }
        public int Xi_rho { get; set; }
        public bool Valid { get; set; }
        public bool ExtraWeight { get; set; }

        public EtaXi(int eta, int xi, bool valid)
        {
            ExtraWeight = false;
            Eta_rho = eta;
            Xi_rho = xi;
            Valid = valid;
        }

        public EtaXi(int eta, int xi, bool valid, bool extraWeight)
        {
            ExtraWeight = extraWeight;
            Eta_rho = eta;
            Xi_rho = xi;
            Valid = valid;
        }
    }

    
}
