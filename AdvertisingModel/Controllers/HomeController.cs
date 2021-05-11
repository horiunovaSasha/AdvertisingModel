using AdvertisingModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisingModel.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        UserManager<CustomUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<CustomUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("admin")) {
                return View("UserList", _userManager.Users.ToList());
            }

            ViewBag.User = await _userManager.FindByNameAsync(User.Identity.Name);
            return View();
        }

        public async Task<IActionResult> Graph(string userId)
        {
            ViewBag.UserId = userId;
            ViewBag.User = await _userManager.FindByIdAsync(userId);
            return View("Index");
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    R = 1.5,
                    P = 1000,
                    C = 678,
                    A = 2000,
                    K0 = 0.65,
                    K1 = 1.25,
                    K2 = 1.85,
                    K3 = 2.45,
                    Func = "-2*x + 4*x^2/3 - 7"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
