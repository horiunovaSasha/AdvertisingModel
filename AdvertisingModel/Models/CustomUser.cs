using Microsoft.AspNetCore.Identity;

namespace AdvertisingModel.Models
{
    public class CustomUser : IdentityUser
    {
        public double R { get; set; } 
        public double P { get; set; } 
        public double C { get; set; } 
        public double A { get; set; }
        public double K0 { get; set; } 
        public double K1 { get; set; }
        public string Func { get; set; }
    }
}
