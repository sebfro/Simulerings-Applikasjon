using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class Fish
    {
        public string id { get; set; }
        public string releaseDate { get; set; }
        public string captureDate { get; set; }
        public double releaseLat { get; set; }
        public double releaseLon { get; set; }
        public double captureLat { get; set; }
        public double captureLon { get; set; }
        public List<TagData> tagDataList { get; set; }
        public List<FishRoute> FishRouteList { get; set; }


        public Fish(string id, string releaseDate, string captureDate, string releaseLatLon, string captureLatLon)
        {
            this.id = id;
            this.captureDate = captureDate;
            this.releaseDate = releaseDate;
            releaseLat = stringToDoubleLat(releaseLatLon);
            releaseLon = stringToDoubleLon(releaseLatLon);
            captureLat = stringToDoubleLat(captureLatLon);
            captureLon = stringToDoubleLon(captureLatLon);
            tagDataList = new List<TagData>();
            FishRouteList = new List<FishRoute>();
        }

        public double stringToDoubleLat(string latAndLon)
        {
            string[] strArray;
            //new[] {',',' '}
            strArray = latAndLon.Split(new char[] { ',' });
            return double.Parse(strArray[0], CultureInfo.InvariantCulture);
        }

        public double stringToDoubleLon(string latAndLon)
        {
            string[] strArray;
            //new[] {',',' '}
            strArray = latAndLon.Split(new char[] { ',' });
            return double.Parse(strArray[1], CultureInfo.InvariantCulture);
        }
    }
}
