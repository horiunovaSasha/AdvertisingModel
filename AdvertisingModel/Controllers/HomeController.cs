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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
