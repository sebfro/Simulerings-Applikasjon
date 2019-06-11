using System;
using System.Globalization;

namespace SimuleringsApplikasjonen
{
    class TagData
    {
        public string Year { get; set; }
        public string Month { get; set; }
        public string Day { get; set; }
        public double Temp { get; set; }
        public double Depth { get; set; }
        public string Date { get; set; }

        public TagData(string line)
        {
            string[] strArray = line.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            Year = strArray[0];
            Month = strArray[1];
            Day = strArray[2];
            Date = Year + Month + Day;
            Temp = double.Parse(strArray[4], CultureInfo.InvariantCulture);
            //Minus foran for å gjøre den negativ
            Depth = -(double.Parse(strArray[5], CultureInfo.InvariantCulture));
        }
    }
}
