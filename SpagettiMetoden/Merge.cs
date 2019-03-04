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
        
        public Merge()
        {

        }

        public Merge(string id, string lat, string lon)
        {
            Id = id;
            Lat = double.Parse(lat);
            Lon = double.Parse(lon);
        }

        public static List<Merge> ReadDirectory(string filePath)
        {
            int intLocation;
            string lastLine;
            List<Merge> List = new List<Merge>();
            Console.WriteLine(filePath + " " + Directory.EnumerateFiles(filePath, "*.txt").Count());
            foreach (string file in Directory.EnumerateFiles(filePath, "*.txt"))
            {
                lastLine = File.ReadLines(file).Last();
                string[] strArray = lastLine.Split('\t');

                intLocation = file.IndexOf("742");

                List.Add(new Merge(file.Substring(intLocation), strArray[0], strArray[1]));
            }
            return List;
        }

        public static void MergeFwAndBwFiles(double increment, double dayInc)
        {
            List<Merge> ForwardList = ReadDirectory(GlobalVariables.pathToFwDirectory);
            List<Merge> BackwardList = ReadDirectory(GlobalVariables.pathToBwDirectory);
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

        public static bool MergeFiles(string fwId, string bwId, int counter)
        {
            try
            {
                string pathToFwFile = GlobalVariables.pathToFwDirectory + fwId;
                string pathToBwFile = GlobalVariables.pathToBwDirectory + bwId;

                List<string> forwardList = new List<string>(File.ReadAllLines(pathToFwFile));
                List<string> backwardList = new List<string>(File.ReadAllLines(pathToBwFile));

                backwardList.RemoveAt(0);
                backwardList.Reverse();

                forwardList.AddRange(backwardList);

                if (counter == 1)
                {
                    DirectoryInfo di = new DirectoryInfo(GlobalVariables.pathToMergedDirectory);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
                
                TextWriter tw = new StreamWriter(GlobalVariables.pathToMergedDirectory + counter + ".txt");

                foreach (string s in forwardList)
                    tw.WriteLine(s);

                tw.Close();

                return true;

            } catch
            {
                return false;
            }
        }

        
    }
}
