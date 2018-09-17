using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class TagData
    {
        public string date { get; set; }
        public string time { get; set; }
        public double temp { get; set; }
        public double depth { get; set; }

        public TagData(string date, string time, string temp, string depth)
        {
            this.date = date;
            this.time = time;
            this.temp = double.Parse(temp, CultureInfo.InvariantCulture);
            this.depth = double.Parse(depth, CultureInfo.InvariantCulture);
        }
    }
}
