using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpagettiMetoden
{
    class FishRoute
    {
        public string id { get; set; }
        public bool alive { get; set; }
        public BlockingCollection<PositionData> PositionDataList { get; set; }

        public FishRoute(string id)
        {
            this.id = id;
            alive = true;
            PositionDataList = new BlockingCollection<PositionData>();
        }
        //Mord fisken, lurte du virkelig?
        public void commitNotAlive()
        {
            alive = false;
        }

        public string[] fromListToString()
        {
            int counter = 1;
            string[] strArray = new string[PositionDataList.Count + 1];
            strArray[0] = "Latitude:" + "\t" + "Longitude:";
            foreach (PositionData pData in PositionDataList)
            {
                strArray[counter] = pData.lat + "\t" + pData.lon;
                counter++;
            }

            return strArray;


        }

        /*
         public string[] fromListToString()
        {
            int counter = 1;
            string[] strArray = new string[PositionDataList.Count+1];
            strArray[0] = "Latitude:" + "\t" + "Longitude:" + "\t" + "Tagdata Depth:" + "\t" + "Sea depth:" + "\t" + "Tagdata Temp" + "\t" + "Sea temp";
            foreach (PositionData pData in PositionDataList)
            {
               strArray[counter] = pData.lat + "\t" + pData.lon + "\t" + pData.tagDataDepth + "\t" + pData.depth +  "\t" + pData.tagDataTemp + "\t" + pData.temp;
                counter++;
            }

            return strArray;


        }
         */
    }
}
