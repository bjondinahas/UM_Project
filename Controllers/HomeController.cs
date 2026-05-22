using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Redirect("/Identity/Account/Login");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
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

            return Redirect("/Identity/Account/Login");
        }

        public IActionResult Features() => Redirect("/Identity/Account/Login");

        public IActionResult About() => Redirect("/Identity/Account/Login");

        public IActionResult Contact() => Redirect("/Identity/Account/Login");

        public IActionResult Privacy() => Redirect("/Identity/Account/Login");

        [Authorize]
        public IActionResult SystemGuide() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
