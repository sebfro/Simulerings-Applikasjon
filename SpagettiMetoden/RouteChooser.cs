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

        //TODO Vi må google eller snakke med veileder og finne ut om denne metoden er trådsikker
        public bool ChosenRoute(BlockingCollection<PositionData> validPositionsDataList, int randInt)
        {
            randDouble = ThreadSafeRandom.NextDouble();

            double newDistanceFromCapture = CalculateCoordinates.GetDistanceFromLatLonInKm(
                                        validPositionsDataList.ElementAt(randInt).Lat,
                                        validPositionsDataList.ElementAt(randInt).Lon, 
                                        captureOrReleaseLat,
                                        captureOrReleaseLon);
            bool extraWeigth = validPositionsDataList.ElementAt(randInt).ExtraWeigth;
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
    }
}