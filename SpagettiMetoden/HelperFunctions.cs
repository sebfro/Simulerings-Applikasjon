using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuleringsApplikasjonen
{
    class HelperFunctions
    {

        public static void DeleteFolderContent(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);


            if(di.GetFiles().Count() > 0)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        public static void DisplayStatisticsOfSimulation(double elapsedMs, int deadFishCounter, int ReleasedFish)
        {
            Console.WriteLine();
            Console.WriteLine("Program runtime: {0} minutes / {1} seconds.", elapsedMs / 60000, elapsedMs / 1000);
            Console.WriteLine("Number of failed routes:      {0}", deadFishCounter);
            Console.WriteLine("Number of successfull routes: {0}", ReleasedFish - deadFishCounter);
        }
    }
}
