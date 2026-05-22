using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using UM_Project.Models;
using UM_Project.Helpers;
using UM_Project.Services.Interfaces;

namespace UM_Project.Areas.Identity.Pages.Account
{
    [EnableRateLimiting("auth")]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthAuditService _auditService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAuthAuditService auditService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/") ?? "/";
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/") ?? "/";
            var ip = HttpContext.GetClientIp();
            var ua = Request.Headers.UserAgent.ToString();

            if (!ModelState.IsValid)
            {
                await _auditService.LogAsync(AuthEventTypes.LoginFailed, false, Input.Email,
                    failureReason: "Validation failed", ipAddress: ip, userAgent: ua);
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email.Trim());
            if (user == null)
            {
                await _auditService.LogAsync(AuthEventTypes.LoginFailed, false, Input.Email,
                    failureReason: "User not found", ipAddress: ip, userAgent: ua);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName ?? user.Email ?? Input.Email.Trim(),
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _auditService.LogAsync(AuthEventTypes.LoginSuccess, true, Input.Email, user.Id,
                    ipAddress: ip, userAgent: ua);

                if (user?.MustChangePassword == true)
                    return LocalRedirect("/Identity/Account/ChangePassword");

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(RoleNames.SuperAdmin) || roles.Contains(RoleNames.Admin))
                        return LocalRedirect("/Admin/Dashboard");
                    if (roles.Contains(RoleNames.Professor))
                        return LocalRedirect("/Professor/Dashboard");
                    if (roles.Contains(RoleNames.Student))
                        return LocalRedirect("/Student/Dashboard");
                    if (roles.Contains(RoleNames.Parent))
                        return LocalRedirect("/Parent/Dashboard");
                }
                return LocalRedirect(ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                await _auditService.LogAsync(AuthEventTypes.AccountLocked, false, Input.Email,
                    failureReason: "Account locked", ipAddress: ip, userAgent: ua);
                ModelState.AddModelError(string.Empty,
                    "Too many failed attempts. Please try again in 15 minutes.");
            }
            else
            {
                await _auditService.LogAsync(AuthEventTypes.LoginFailed, false, Input.Email,
                    failureReason: "Invalid credentials", ipAddress: ip, userAgent: ua);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return Page();
        }
    }
}
