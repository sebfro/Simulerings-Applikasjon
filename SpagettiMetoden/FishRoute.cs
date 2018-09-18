using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class FishRoute
    {
        public string id { get; set; }
        public bool alive { get; set; }
        public List<PositionData> PositionDataList { get; set; }

        public FishRoute(string id)
        {
            this.id = id;
            alive = true;
            PositionDataList = new List<PositionData>();
        }
        //Mord fisken, lurte du virkelig?
        public void commitNotAlive()
        {
            alive = false;
        }
    }
}
