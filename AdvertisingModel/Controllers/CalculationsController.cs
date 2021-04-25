using Microsoft.AspNetCore.Mvc;
using org.mariuszgromada.math.mxparser;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;
using System.Text.RegularExpressions;
using CustomExpression = MathNet.Symbolics.SymbolicExpression;

namespace AdvertisingModel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationsController : Controller
    {
        private static string _userFunctionText = "4 * x * x / 3 - 2 * x - 7";
        
        public CalculationsController()
        {
        }

        //"2+(3-5)^2"
        [HttpGet]
        [Route("ChangeUserFunction")]
        public string ChangeUserFunction(string textForm)
        {
            try
            {
                _userFunctionText = textForm;

                return _userFunctionText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }



       // q(t) = -2*x + 4x^2/3 - 7;
        [HttpGet()]
        [Route("Calculate")]
        public Vector<double>[] Calculate(int n)
        {
            int N = 100;
            double r = 1.5;// R(0)
            int t_first = 0;
            int t_end = 1;
            double p = 1000;//ціна за якою продаємо
            double c = 678;//собівартість (за елекроенергію, ...)
            double a = 2000; //виділяємо грошей на рекламу 
            double k0 = 0.65;//k(t) - коефіцієнт "набридання" реклами
            double k1 = 1.25;

            Vector<double> variables = Vector<double>.Build.Dense(new[] { 0, r });
            Func<double, Vector<double>, Vector<double>> der = DerivativeMaker(p, c, a, k0);

            Vector<double>[] res = RungeKutta.FourthOrder(variables, t_first, t_end, N, der);

            double[] P = new double[N];
            double[] R = new double[N];
            int i = 0;

            foreach (var x in res)
            {
                P[i] = x[0];
                R[i] = x[1];
                Console.WriteLine("P " + P[i] + "\t R " + R[i]);
                i++;
            }
            double[] t = function_t(t_first, t_end, N);

            var R0 = function_R0(p, c, k0, k1);
            return res;
        }

        //runge kutta
        static Func<double, Vector<double>, Vector<double>> DerivativeMaker(double p, double c, double a, double k0)
        {
            return (t, Z) =>
            {
                double[] A = Z.ToArray();
                double P = A[0];
                double x = A[1];
                
                var userFunction = CustomExpression.Parse(_userFunctionText.Replace("x", x.ToString()));

                return Vector<double>.Build.Dense(new[] { (p - c) * userFunction.ComplexNumberValue.Real - a,
                                                         k0 * a - k0 * x + 1
                                                        });
            };
        }

        //розбиття t
        static double[] function_t(int t_first, int t_end, int n)
        {
            double[] t = new double[n];
            t[0] = 0;
            for (int i = 1; i < n; i++)
            {
                t[i] = t[i - 1] + (double)(t_end - t_first) / n;
            }
            return t;
        }

        static double function_R0(double p, double c, double k0, double k1)
        {
            var x = CustomExpression.Variable("x");
            var userFunction = CustomExpression.Parse(_userFunctionText);

            var derivative = userFunction.Differentiate(x);     //q'(x)

            var equation = derivative - k1 / k0 * 1 / (p - c); //q'(x) - k1/k0*1/(p-c) = 0
            var R0 = Convert.ToDouble(Regex.Matches((-equation[0] / equation[1]).ToString(), @"(\-)?\d+(\.\d+)?")[0].ToString());//find R from equation and then extract double

            return R0;
        }
    }
}
