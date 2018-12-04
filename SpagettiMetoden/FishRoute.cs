using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpagettiMetoden
{
    class FishRoute
    {
        public string Id { get; set; }
        public bool Alive { get; set; }
        public BlockingCollection<PositionData> PositionDataList { get; set; }

        public FishRoute(string id)
        {
            Id = id;
            Alive = true;
            PositionDataList = new BlockingCollection<PositionData>();
        }
        //Mord fisken, lurte du virkelig?
        public void CommitNotAlive()
        {
            Alive = false;
        }
        /*
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
        */
        
         public string[] FromListToString()
        {
            int counter = 1;
            string[] strArray = new string[PositionDataList.Count+1];
            strArray[0] = "Latitude:" + "\t" + "Longitude:" + "\t" + "Tagdata Depth:" + "\t" + "Sea depth:" + "\t" + "Tagdata Temp" + "\t" + "Sea temp";
            foreach (PositionData pData in PositionDataList)
            {
               strArray[counter] = pData.Lat + "\t" + pData.Lon + "\t" + pData.TagDataDepth + "\t" + pData.Depth +  "\t" + pData.TagDataTemp + "\t" + pData.Temp;
                counter++;
            }

            return strArray;


        }
        
    }
}
