using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class ReadFromFile
    {
        public void readReleaseAndCapture(Dictionary<string, Fish> FishList, List<string> KeyList)
        {
            string line;

            StreamReader file = new StreamReader(GlobalVariables.pathToNcTagData + "DataOversiktTekst.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] strArray;
                strArray = line.Split('\t');
                Fish currentFish = new Fish(strArray[0], strArray[1], strArray[2], strArray[3], strArray[4]);
                FishList[strArray[0]] = currentFish;

                KeyList.Add(strArray[0]);
            }
            file.Close();
        }

        public void readTagData(Dictionary<string, Fish> FishList, List<string> KeyList)
        {
            foreach (string id in KeyList)
            {
                string tagFileName = Directory.GetFiles(GlobalVariables.pathToNcTagData + @"Final_DST\10min_Sampling\Sampling_10min_2000s_Files\", "Tag" + id + "asc.DATA")[0];
                if (tagFileName != null)
                {
                    List<TagData> tagDataArray = new List<TagData>();
                    string line = "";
                    StreamReader file = new StreamReader(tagFileName);
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] strArray = line.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        TagData tagData = new TagData(line);
                        tagDataArray.Add(tagData);
                        FishList[id].tagDataList = tagDataArray;
                    }
                    file.Close();
                }
                else
                {
                    //Fjerner key of fiske objekt hvis det ikke er merkedata for den fisken tilgjengelig
                    FishList.Remove(id);
                    KeyList.Remove(id);
                }
            }
        }
    }
}
