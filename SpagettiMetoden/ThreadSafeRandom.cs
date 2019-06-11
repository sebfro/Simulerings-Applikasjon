using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuleringsApplikasjonen
{
    public static class ThreadSafeRandom
    {
        private static Random _inst = new Random();

        public static int Next(int range)
        {
            lock (_inst) return _inst.Next(range);
        }
        public static double NextDouble()
        {
            lock (_inst) return _inst.NextDouble();
        }
        public static double RandomDouble(float min, float max)
        {
            lock (_inst) return _inst.NextDouble() * (max - min) + 0.4;
        }
    }
}
