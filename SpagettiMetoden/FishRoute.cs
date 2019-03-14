using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpagettiMetoden
{
    class FishRoute
    {
        public string Id { get; set; }
        public bool Alive { get; set; }
        public bool Use_Norkyst { get; set; }
        public BlockingCollection<PositionData> PositionDataList { get; set; }

        public FishRoute(string id, bool Use_Norkyst)
        {
            this.Use_Norkyst = Use_Norkyst;
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
        
         public string[] FromListToString(string[] dateTable)
        {
            int counter = 1;
            string[] strArray = new string[PositionDataList.Count+1];
            strArray[0] = "Latitude:" + "\t" + "Longitude:" + "\t" + "Tagdata Depth:" + "\t" + "Sea depth:" + "\t" + "Tagdata Temp" + "\t" + "Sea temp" + "\t" + "Date";
            foreach (PositionData pData in PositionDataList)
            {
               strArray[counter] = pData.Lat + "\t" + pData.Lon + "\t" + pData.TagDataDepth + "\t" + pData.Depth +  "\t" + pData.TagDataTemp + "\t" + pData.Temp + "\t" + dateTable[counter - 1];
                counter++;
            }

            return strArray;


        }
        
    }
}
