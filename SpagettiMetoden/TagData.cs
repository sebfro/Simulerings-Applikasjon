using System;
using System.Globalization;

namespace SpagettiMetoden
{
    class TagData
    {
        public string year { get; set; }
        public string month { get; set; }
        public string day { get; set; }
        public double temp { get; set; }
        public double depth { get; set; }

        public TagData(string line)
        {
            string[] strArray = line.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            year = strArray[0];
            month = strArray[1];
            day = strArray[2];
            temp = double.Parse(strArray[4], CultureInfo.InvariantCulture);
            //Minus foran for å gjøre den negativ
            depth = -(double.Parse(strArray[5], CultureInfo.InvariantCulture));
        }
    }
}
