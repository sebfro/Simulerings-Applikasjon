using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class RouteChooser
    {
        public double currDistanceFromCaptureOrRelease;
        public double randDouble;
        public double captureOrReleaseLat;
        public double captureOrReleaseLon;

        public RouteChooser(double goalLat, double goalLon, double lat, double lon)
        {
            currDistanceFromCaptureOrRelease = CalculateCoordinates.GetDistanceFromLatLonInKm(lat, lon, goalLat, goalLon);
            captureOrReleaseLat = goalLat;
            captureOrReleaseLon = goalLon;

        }
        //Lat: 77 og Lon: 53
        //Rreturnerer sant hvis den velger indeksen for posisjonen som ble sendt, ellers rerturneres falsk
        //Hvis vektingen er 0 så returnerer den alltid true.
        public bool ChosenRoute(BlockingCollection<PositionData> validPositionsDataList, int randInt)
        {
            randDouble = ThreadSafeRandom.NextDouble();

            double newDistanceFromCapture = CalculateCoordinates.GetDistanceFromLatLonInKm(
                                        validPositionsDataList.ElementAt(randInt).Lat,
                                        validPositionsDataList.ElementAt(randInt).Lon,
                                        captureOrReleaseLat,
                                        captureOrReleaseLon);
            //bool extraWeigth = validPositionsDataList.ElementAt(randInt).ExtraWeigth;
            double weight = GlobalVariables.Probability;

            if(weight == 0)
            {
                return true;
            } else
            {
                return ((newDistanceFromCapture < currDistanceFromCaptureOrRelease && randDouble < weight) ||
                    (newDistanceFromCapture >= currDistanceFromCaptureOrRelease && randDouble >= weight));
            }

            //Denne versjonen var for havtrøm implementasjonen som gjorde at fisken følgte havstrømmen 
            //return (newDistanceFromCapture < currDistanceFromCaptureOrRelease && randDouble < (extraWeigth ? weight + 0.4 : weight) || 
            //    newDistanceFromCapture >= currDistanceFromCaptureOrRelease && randDouble >= (extraWeigth ? weight - 0.4 : weight));
        }
        //Går gjennom all posisjonene i validPositionsDataList og finner den med temp mærmest DST temp
        public int ChoosePosWithClosestTemp(BlockingCollection<PositionData> validPositionsDataList, double DSTtemp)
        {
            int index = 0;
            double currTempDelta = 0;
            for (int i = 0; i < validPositionsDataList.Count; i++)
            {
                if(i == 0)
                {
                    currTempDelta = Math.Abs(validPositionsDataList.ElementAt(i).Temp - DSTtemp);
                }
                else
                {
                        double localTempDelta = Math.Abs(validPositionsDataList.ElementAt(i).Temp - DSTtemp);
                    if (currTempDelta > localTempDelta)
                        {
                        index = i;
                        currTempDelta = localTempDelta;
                        }
                }
            }
            return index;
        }
    }
}