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
        public async Task<CalculationViewModel> Calculate(string userId, double r = 1.5, double p = 1000, double c = 678, double a = 2000, double k0 = 0.65, double k1 = 1.25, double k2 = 1.85, double k3 = 2.45, string func = "-2*x + 4*x^2/3 - 7")
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
                user.K2 = k2;
                user.K3 = k3;
                user.Func = _userFunctionText;


                await _userManager.UpdateAsync(user);
                ViewBag.User = user;
            }

            return CalculateFunc(r, p, c, a, k0, k1, k2, k3);
        }

        [HttpGet()]
        [Route("GetCalculate")]
        public async Task<CalculationViewModel> GetCalculate()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            _userFunctionText = user.Func;
            ViewBag.User = user;

            return CalculateFunc(user.R, user.P, user.C, user.A, user.K0, user.K1, user.K2, user.K3);
        }

        private CalculationViewModel CalculateFunc(double r, double p, double c, double a_max, double k0, double k1, double k2, double k3)
        {
            int N = 10;
            int T = 1;
            int t_first = 0;

            var result = new CalculationViewModel();

            //[0,T]
            Vector<double>[] res = function_RungeKutta(0, r, p, c, a_max, k0, 0, T, N);

            double[] P = new double[N];
            double[] R = new double[N];
            int i = 0;
            foreach (var x in res)
            {
                P[i] = x[0];
                R[i] = x[1];

                result.Zero_T.P[i] = x[0];
                result.Zero_T.R[i] = x[1];
                i++;
            }

            double[] t = function_t(t_first, T, N);

            var R0 = function_R0(p, c, k0, k1);
            var T1 = function_T1(R0, r, k0, a_max);
            var T2 = function_T2(T1, R, k0, k1, a_max, R0, p, c, N, t);
            var a_opt = function_a_opt(T1, R, k0, k1, a_max, R0, p, c, N, t);

            //[0,T1]
            res = function_RungeKutta(P[P.Length - 1], R[R.Length - 1], p, c, a_max, k1, 0, T1, N);
            i = 0;

            foreach (var x in res)
            {
                P[i] = x[0];
                R[i] = x[1];

                result.Zero_T1.P[i] = x[0];
                result.Zero_T1.R[i] = x[1];
                i++;
            }

            //[T1,T-T2]
            res = function_RungeKutta(P[P.Length - 1], R[R.Length - 1], p, c, a_max, k2, T1, T - T2, N);
            i = 0;

            foreach (var x in res)
            {
                P[i] = x[0];
                R[i] = x[1];

                result.T1_T2.P[i] = x[0];
                result.T1_T2.R[i] = x[1];
                i++;
            }

            //[T-T2,T]
            res = function_RungeKutta(P[P.Length - 1], R[R.Length - 1], p, c, a_max, k3, T - T2, T, N);
            i = 0;

            foreach (var x in res)
            {
                P[i] = x[0];
                R[i] = x[1];

                result.T2_T.P[i] = x[0];
                result.T2_T.R[i] = x[1];
                i++;
            }

            //return res;
            return result;
        }

        static Vector<double>[] function_RungeKutta(double P, double R, double p, double c, double a, double k,
                                        double t_first, double t_end, int N)
        {
            Vector<double> variables = Vector<double>.Build.Dense(new[] { P, R });

            Func<double, Vector<double>, Vector<double>> der = DerivativeMaker(p, c, a, k);

            Vector<double>[] res = RungeKutta.FourthOrder(variables, t_first, t_end, N, der);

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
            var func = CustomExpression.Parse(_userFunctionText); //q(x)
            var derivative = func.Differentiate(x);     //q'(x)

            var equation = derivative - k1 / k0 * 1 / (p - c); // - k1/k0*1/(p-c) + q'(x) = 0
            var R0 = Convert.ToDouble(Regex.Matches((-equation[0] / equation[1]).ToString(), @"(\-)?\d+(\.\d+)?")[0].ToString());//find R from equation and then extract double

            return R0;
        }


        static double function_T1(double R0, double r, double k0, double a_max)
        {
            var T1 = Math.Log(Math.Abs((R0 - k0 * a_max) / (r - k0 * R0))) / k0;
            return T1;
        }

        static double function_Simpson(double a, double b, double N, double[] R, double p, double c,
                                        double a_max, int i_first, int i_end)
        {
            double S, S1, S2 = 0, S3 = 0, h;
            int i = 0, j, z = 0;

            double[] Y = new double[R.Length];
            h = (b - a) / N;

            for (i = i_first + 1; i < i_end; i++)
            {
                if (i_first > 0)
                {
                    j = i - i_end + i_first;//first integral
                }
                else
                {
                    j = i - 1;//second integral
                }
                Y[z] = (p - c) * (4 * R[j] * R[j] / 3 - 2 * R[j] + 7);//(p - c) * q(R(t))
                z++;
            }

            S1 = Y[0] + Y[R.Length - 1];

            for (i = 0; i < R.Length; i++)
            {
                if (i % 2 != 0)
                {
                    S3 += Y[i];
                }
                else
                {
                    S2 += Y[i];
                }
            }

            S = h / 3 * (S1 + 4 * S2 + 2 * S3); //Simpson

            return S;
        }

        static double function_T2(double T1, double[] R, double k0, double k1, double a_max,
                                    double R0, double p, double c, double N, double[] t)
        {
            double a, b, S1, S2, T2 = 0;
            int i_first = 0, i_end = 0;

            for (int i = 1; i < t.Length; i++)
            {
                b = t[t.Length - 1];
                a = b - t[i];
                i_first = t.Length - 1 - i;
                i_end = t.Length - 1;

                S1 = function_Simpson(a, b, N, R, p, c, a_max, i_first, i_end);//first integral

                b = t[i];
                a = 0;
                i_first = 0;
                i_end = i;

                S2 = function_Simpson(a, b, N, R, p, c, a_max, i_first, i_end);//second integral

                if (S1 - S2 > 0)
                {
                    T2 = (t[i] - t[i - 1]) / 2;
                    break;
                }
            }
            return T2;
        }

        static double function_a_opt(double T1, double[] R, double k0, double k1, double a_max,
                                    double R0, double p, double c, double N, double[] t)
        {
            int i_first = 0, i_end = t.Length - 1;
            double a, b;

            b = t[t.Length - 1];
            a = 0;

            var S = function_Simpson(a, b, N, R, p, c, a_max, i_first, i_end);
            var a_opt = S * k1 / k0;

            return a_opt;
        }

       
    }
}
