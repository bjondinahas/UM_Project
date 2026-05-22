using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // User not found in database, sign out
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index");
                }
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(RoleNames.SuperAdmin) || roles.Contains(RoleNames.Admin))
                    return RedirectToAction("Dashboard", "Admin");
                if (roles.Contains(RoleNames.Professor))
                    return RedirectToAction("Dashboard", "Professor");
                if (roles.Contains(RoleNames.Student))
                    return RedirectToAction("Dashboard", "Student");
                if (roles.Contains(RoleNames.Parent))
                    return RedirectToAction("Dashboard", "Parent");
            }
            return View();
        }

        public IActionResult Features() => View();

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        public IActionResult Privacy() => View();

        [Authorize]
        public IActionResult SystemGuide() => View();
    }
}
