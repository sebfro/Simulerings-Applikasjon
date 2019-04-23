using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagettiMetoden
{
    class Merge
    {
        public string Id { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        private string PathToDirectory { get; set; }

        public Merge()
        {

        }
        public Merge(string id)
        {
            Id = id;
            PathToDirectory = @"C:\NCdata\fishData\";
        }

        public Merge(string id, string lat, string lon)
        {
            Id = id;
            Lat = double.Parse(lat);
            Lon = double.Parse(lon);
        }

        public List<Merge> ReadDirectory(string filePath)
        {
            int intLocation;
            string lastLine;
            List<Merge> List = new List<Merge>();
            Console.WriteLine(filePath + " " + Directory.EnumerateFiles(filePath, "*.txt").Count());
            foreach (string file in Directory.EnumerateFiles(filePath, "*.txt"))
            {
                lastLine = File.ReadLines(file).Last();
                string[] strArray = lastLine.Split('\t');

                intLocation = file.IndexOf(Id);

                List.Add(new Merge(file.Substring(intLocation), strArray[0], strArray[1]));
            }
            return List;
        }

        public void MergeFwAndBwFiles(double increment, double dayInc)
        {
            List<Merge> ForwardList = ReadDirectory(PathToDirectory  + Id + @"\FW");
            List<Merge> BackwardList = ReadDirectory(PathToDirectory  + Id + @"\BW");
            int counter = 1;
            bool found;

            foreach (var fwPos in ForwardList)
            {
                found = false;
                for (int i = 0; i < BackwardList.Count && !found; i++)
                {
                    if (CalculateCoordinates.GetDistanceFromLatLonInKm(fwPos.Lat, fwPos.Lon, BackwardList[i].Lat, BackwardList[i].Lon) < (increment * 3.6 * (dayInc * 24)))
                    {
                        if (MergeFiles(fwPos.Id, BackwardList[i].Id, counter))
                        {
                            BackwardList.RemoveAt(i);
                            counter++;
                            found = true;
                        }
                    }
                }
            }
            Console.WriteLine("Finished merging files");
        }

        public bool MergeFiles(string fwId, string bwId, int counter)
        {
            try
            {
                string pathToFwFile = PathToDirectory + fwId;
                string pathToBwFile = PathToDirectory + bwId;

                List<string> forwardList = new List<string>(File.ReadAllLines(pathToFwFile));
                List<string> backwardList = new List<string>(File.ReadAllLines(pathToBwFile));

                backwardList.RemoveAt(0);
                backwardList.Reverse();

                forwardList.AddRange(backwardList);

                string DirecotryPath = @"C:\NCdata\fishData\" + Id + @"\Akseptabel";

                if (counter == 1)
                {
                    HelperFunctions.DeleteFolderContent(DirecotryPath);
                }
                
                TextWriter tw = new StreamWriter(DirecotryPath + @"\" + Id + @"_" + counter + ".txt");

                foreach (string s in forwardList)
                    tw.WriteLine(s);

                tw.Close();

                return true;

            } catch
            {
                Console.WriteLine(PathToDirectory + fwId);
                Console.WriteLine(fwId);
                Console.WriteLine(PathToDirectory + bwId);
                Console.WriteLine(bwId);
                Console.WriteLine(@"C:\NCdata\fishData\" + Id + @"\Akseptabel\" + Id + @"_" + counter + ".txt");
                return false;
            }
        }

        
    }
}
