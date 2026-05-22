using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UM_Project.Models;

namespace UM_Project.Areas.Identity.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool IsRequired { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });
            IsRequired = user.MustChangePassword;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            IsRequired = user.MustChangePassword;

            if (!ModelState.IsValid) return Page();

            var changeResult = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var error in changeResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Password updated successfully.";

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(RoleNames.SuperAdmin) || roles.Contains(RoleNames.Admin))
                return RedirectToAction("Dashboard", "Admin");
            if (roles.Contains(RoleNames.Professor))
                return RedirectToAction("Dashboard", "Professor");
            if (roles.Contains(RoleNames.Student))
                return RedirectToAction("Dashboard", "Student");
            if (roles.Contains(RoleNames.Parent))
                return RedirectToAction("Dashboard", "Parent");

            return RedirectToAction("Index", "Home");
        }
    }
}
