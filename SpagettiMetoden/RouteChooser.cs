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
        public double currDistanceFromCapture;
        public double randDouble;
        public double captureLat;
        public double captureLon;

        public RouteChooser(double lat, double lon, Fish currFish)
        {
            currDistanceFromCapture = CalculateCoordinates.GetDistanceFromLatLonInKm(lat, lon, currFish.CaptureLat, currFish.CaptureLon);
            captureLat = currFish.CaptureLat;
            captureLon = currFish.CaptureLon;
        }
        //TODO Vi må google eller snakke med veileder og finne ut om denne metoden er trådsikker
        public bool ChosenRoute(BlockingCollection<PositionData> validPositionsDataList, int randInt)
        {
            randDouble = ThreadSafeRandom.NextDouble();

            double newDistanceFromCapture = CalculateCoordinates.GetDistanceFromLatLonInKm(
                                        validPositionsDataList.ElementAt(randInt).lat,
                                        validPositionsDataList.ElementAt(randInt).lon, captureLat,
                                        captureLon);
            double weight = GlobalVariables.Probability;
            return (newDistanceFromCapture < currDistanceFromCapture && randDouble < weight || newDistanceFromCapture >= currDistanceFromCapture && randDouble >= weight);
        }
    }
}