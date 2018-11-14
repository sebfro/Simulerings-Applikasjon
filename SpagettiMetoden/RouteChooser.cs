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
        public double captureLat;
        public double captureLon;

        public RouteChooser(double lat, double lon, Fish currFish)
        {
            currDistanceFromCaptureOrRelease = CalculateCoordinates.GetDistanceFromLatLonInKm(lat, lon, currFish.CaptureLat, currFish.CaptureLon);
            captureLat = currFish.CaptureLat;
            captureLon = currFish.CaptureLon;

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
                                        validPositionsDataList.ElementAt(randInt).lat,
                                        validPositionsDataList.ElementAt(randInt).lon, captureOrReleaseLat,
                                        captureOrReleaseLon);
            double weight = GlobalVariables.Probability;
            return (newDistanceFromCapture < currDistanceFromCaptureOrRelease && randDouble < weight || newDistanceFromCapture >= currDistanceFromCaptureOrRelease && randDouble >= weight);
        }
    }
}