
namespace SimuleringsApplikasjonen
{
    class EtaXi
    {
        public int Eta_rho { get; set; }
        public int Xi_rho { get; set; }
        public bool Valid { get; set; }
        public bool ExtraWeight { get; set; }

        public EtaXi(int eta, int xi, bool valid)
        {
            ExtraWeight = false;
            Eta_rho = eta;
            Xi_rho = xi;
            Valid = valid;
        }

        public EtaXi(int eta, int xi, bool valid, bool extraWeight)
        {
            ExtraWeight = extraWeight;
            Eta_rho = eta;
            Xi_rho = xi;
            Valid = valid;
        }
    }
}
