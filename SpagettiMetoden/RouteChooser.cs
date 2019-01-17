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

        public RouteChooser(double lat, double lon, Fish currFish)
        {
            currDistanceFromCaptureOrRelease = CalculateCoordinates.GetDistanceFromLatLonInKm(lat, lon, currFish.CaptureLat, currFish.CaptureLon);
            captureOrReleaseLat = currFish.CaptureLat;
            captureOrReleaseLon = currFish.CaptureLon;

        }

        public RouteChooser(double lat, double lon, double currFishCaptureOrReleaseLat, double currFishCaptureOrReleaseLon)
        {
            currDistanceFromCaptureOrRelease = CalculateCoordinates.GetDistanceFromLatLonInKm(lat, lon, currFishCaptureOrReleaseLat, currFishCaptureOrReleaseLon);
            captureOrReleaseLat = currFishCaptureOrReleaseLat;
            captureOrReleaseLon = currFishCaptureOrReleaseLon;
        }

        //TODO Vi må google eller snakke med veileder og finne ut om denne metoden er trådsikker
        public bool ChosenRoute(BlockingCollection<PositionData> validPositionsDataList, int randInt)
        {
            randDouble = ThreadSafeRandom.NextDouble();

            double newDistanceFromCapture = CalculateCoordinates.GetDistanceFromLatLonInKm(
                                        validPositionsDataList.ElementAt(randInt).Lat,
                                        validPositionsDataList.ElementAt(randInt).Lon, captureOrReleaseLat,
                                        captureOrReleaseLon);
            bool extraWeigth = validPositionsDataList.ElementAt(randInt).ExtraWeigth;
            double weight = GlobalVariables.Probability;

            return ((newDistanceFromCapture < currDistanceFromCaptureOrRelease && randDouble < weight) ||
                    (newDistanceFromCapture >= currDistanceFromCaptureOrRelease && randDouble >= weight));

            //return (newDistanceFromCapture < currDistanceFromCaptureOrRelease && randDouble < (extraWeigth ? weight + 0.4 : weight) || 
            //    newDistanceFromCapture >= currDistanceFromCaptureOrRelease && randDouble >= (extraWeigth ? weight - 0.4 : weight));
        }
    }
}