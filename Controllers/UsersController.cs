using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public class UsersController : Controller
    {
        private readonly IUiText _ui;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IAdminPasswordService _passwordService;
        private readonly AccountProvisioningSettings _settings;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IAdminPasswordService passwordService,
            IOptions<AccountProvisioningSettings> settings, IUiText ui)
        {
            _ui = ui;

            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _passwordService = passwordService;
            _settings = settings.Value;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }
            ViewBag.UserRoles = userRoles;
            ViewBag.IsSuperAdmin = true;
            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser model, string password, string role)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    CustomId = model.CustomId,
                    Address = model.Address,
                    EmailConfirmed = true,
                    MustChangePassword = true
                };
                var pwd = string.IsNullOrWhiteSpace(password) ? _settings.DefaultPassword : password;
                var result = await _userManager.CreateAsync(user, pwd);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName, pwd, role);
                    TempData["Success"] = _ui.Format("Flash_UserCreated", user.Email, role);
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault();
            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            ViewBag.DefaultPassword = _settings.DefaultPassword;
            ViewBag.CanEditRole = true;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            user.CustomId = model.CustomId;
            user.Address = model.Address;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = _ui["Flash_UserUpdated"];
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string? newPassword, bool requireChange = true)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var pwd = string.IsNullOrWhiteSpace(newPassword) ? _settings.DefaultPassword : newPassword;
            var (ok, err) = await _passwordService.ResetPasswordAsync(id, pwd, requireChange);
            TempData[ok ? "Success" : "Error"] = ok
                ? _ui.Format("Flash_PasswordResetUser", user.Email, pwd)
                : err;
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && user.Email != "superadmin@umproject.com")
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = _ui["Flash_UserDeleted"];
            }
            else
            {
                TempData["Error"] = _ui["Flash_CannotDeleteSuperAdmin"];
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
