using System;
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

            StreamReader file = new StreamReader(@"C:\NCdata\Merkedata\DataOversiktTekst.txt");
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
            //Mappe strukturen er annerledes på HVL pc-en, fiks det slik at den er slik som den er under.
            const string DIRECTORY_PATH = @"C:\NCdata\Merkedata\Final_DST\10min_Sampling\Sampling_10min_2000s_Files\";

            foreach (string id in KeyList)
            {
                string tagFileName = Directory.GetFiles(DIRECTORY_PATH, "Tag" + id + "asc.DATA")[0];
                if (tagFileName != null)
                {
                    List<TagData> tagDataArray = new List<TagData>();
                    string line = "";
                    StreamReader file = new StreamReader(tagFileName);
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] strArray;
                        strArray = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        TagData tagData = new TagData(strArray[0], strArray[1], strArray[2], strArray[3]);
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
