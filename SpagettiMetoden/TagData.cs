using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class TagData
    {
        public string date { get; set; }
        public string time { get; set; }
        public string temp { get; set; }
        public string depth { get; set; }

        public TagData(string date, string time, string temp, string depth)
        {
            this.date = date;
            this.time = time;
            this.temp = temp;
            this.depth = depth;
        }
    }
}
