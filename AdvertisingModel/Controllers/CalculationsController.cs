using Microsoft.AspNetCore.Mvc;
using org.mariuszgromada.math.mxparser;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;
using System.Linq;

namespace AdvertisingModel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationsController : Controller
    {
        //"2+(3-5)^2"
        [HttpGet]
        [Route("ParseAndCalculate")]
        public string ParseAndCalculate(string textForm)
        {
            try
            {
                var result = new Expression(textForm).calculate();
                return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet()]
        [Route("Calculate")]
        public Vector<double>[] Calculate(int n)
        {
            Vector<double> y0 = Vector<double>.Build.Dense(new[] { -7.0 / 4.0, 55.0 / 8.0 });
            Func<double, Vector<double>, Vector<double>> der = DerivativeMaker();

            Vector<double>[] res = RungeKutta.FourthOrder(y0, 0, 1, n, der);

            res.ToList().ForEach(x => Console.WriteLine($"{x.ToRowMatrix()}"));

            return res;
        }


            /*
             x'(t)=x(t)+2y(t)+2t
             y'(t)=3x(t)+2y(t)-4t
             x(0)=-7/4, y(0)=55/8
             x(0)=-1.75, y(0)=6.875

            */

        static Func<double, Vector<double>, Vector<double>> DerivativeMaker()
        {
            return (t, Z) =>
            {
                double[] A = Z.ToArray();
                double x = A[0];
                double y = A[1];

                return Vector<double>.Build.Dense(new[] { x + 2 * y + 2 * t, 3 * x + 2 * y - 4 * t });

            };
        }
    }
}
