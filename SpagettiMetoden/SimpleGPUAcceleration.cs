using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Translator;
using Cudafy.Host;

namespace SpagettiMetoden
{
    class SimpleGPUAcceleration
    {
        public static void startUp()
        {
            CudafyModes.Target = eGPUType.OpenCL;
            CudafyTranslator.Language = eLanguage.OpenCL;
            CudafyModule km = CudafyTranslator.Cudafy();
            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);
            gpu.Launch().thekernel();
            Console.WriteLine("Hello world");
        }

        [Cudafy]
        public static void thekernel()
        {
        }
    }
}
