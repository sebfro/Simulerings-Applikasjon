using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace SimuleringsApplikasjonen
{
    class Fish
    {
        public string Id { get; set; }
        public string ReleaseDate { get; set; }
        public string CaptureDate { get; set; }
        public double ReleaseLat { get; set; }
        public double ReleaseLon { get; set; }
        public double CaptureLat { get; set; }
        public double CaptureLon { get; set; }
        public List<TagData> TagDataList { get; set; }
        public BlockingCollection<FishRoute> FishRouteList { get; set; }


        public Fish(string id, string releaseDate, string captureDate, string releaseLatLon, string captureLatLon)
        {
            Id = id;
            CaptureDate = captureDate;
            ReleaseDate = releaseDate;
            ReleaseLat = StringToDoubleLat(releaseLatLon);
            ReleaseLon = StringToDoubleLon(releaseLatLon);
            CaptureLat = StringToDoubleLat(captureLatLon);
            CaptureLon = StringToDoubleLon(captureLatLon);
            TagDataList = new List<TagData>();
            FishRouteList = new BlockingCollection<FishRoute>();
        }

        public double StringToDoubleLat(string latAndLon)
        {
            string[] strArray;
            //new[] {',',' '}
            strArray = latAndLon.Split(new char[] { ',' });
            return double.Parse(strArray[0], CultureInfo.InvariantCulture);
        }

        public double StringToDoubleLon(string latAndLon)
        {
            string[] strArray;
            //new[] {',',' '}
            strArray = latAndLon.Split(new char[] { ',' });
            return double.Parse(strArray[1], CultureInfo.InvariantCulture);
        }
    }
}
