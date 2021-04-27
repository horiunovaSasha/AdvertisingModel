using Microsoft.AspNetCore.Mvc;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;
using System.Text.RegularExpressions;
using CustomExpression = MathNet.Symbolics.SymbolicExpression;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using AdvertisingModel.Models;

namespace AdvertisingModel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationsController : Controller
    {
        private static string _userFunctionText = "4 * x * x / 3 - 2 * x - 7";
        private readonly UserManager<CustomUser> _userManager;

        public CalculationsController(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
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
        public async Task<Vector<double>[]> Calculate(string userId, double r = 1.5, double p = 1000, double c = 678, double a = 2000, double k0 = 0.65, double k1 = 1.25, string func = "-2*x + 4x^2/3 - 7")
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                _userFunctionText = func;
                user.R = r;
                user.P = p;
                user.C = c;
                user.A = a;
                user.K0 = k0;
                user.K1 = k1;
                user.Func = _userFunctionText;


                await _userManager.UpdateAsync(user);
                ViewBag.User = user;
            }

            return CalculateFunc(r, p, c, a, k0, k1);
        }

        [HttpGet()]
        [Route("GetCalculate")]
        public async Task<Vector<double>[]> GetCalculate()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            _userFunctionText = user.Func;
            ViewBag.User = user;

            return CalculateFunc(user.R, user.P, user.C, user.A, user.K0, user.K1);
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

        private Vector<double>[] CalculateFunc(double r, double p, double c, double a, double k0, double k1)
        {
            int N = 101;
            int t_first = 0;
            int t_end = 1;

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
    }
}
