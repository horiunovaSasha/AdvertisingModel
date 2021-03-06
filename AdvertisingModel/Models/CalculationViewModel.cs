namespace AdvertisingModel.Models
{
    public class CalculationViewModel
    {
        public PR_Pair Zero_T1 {get; set;} = new PR_Pair();
        public PR_Pair T1_T2 {get; set;} = new PR_Pair();
        public PR_Pair T2_T {get; set;} = new PR_Pair();
        public bool IsAlphaMaxValid { get; set; }
        public double T1 { get; set; }
        public double T2 { get; set; }
        public double Opt { get; set; }
    }

    public class PR_Pair 
    {
        public double[] P { get; set; } = new double[10];
        public double[] R { get; set; } = new double[10];
    }
}
