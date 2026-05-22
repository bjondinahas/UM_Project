using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Areas.Identity.Pages.Account
{
    [EnableRateLimiting("auth")]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthAuditService _auditService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your password.")]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/") ?? "/";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/") ?? "/";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            if (!ModelState.IsValid)
            {
                await _auditService.LogAsync(AuthEventTypes.RegisterFailed, false, Input.Email,
                    failureReason: "Validation failed", ipAddress: ip, userAgent: ua);
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = Input.Email.Split('@')[0],
                CustomId = "USR" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Address = ""
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                await _auditService.LogAsync(AuthEventTypes.RegisterFailed, false, Input.Email,
                    failureReason: errors, ipAddress: ip, userAgent: ua);
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            await _auditService.LogAsync(AuthEventTypes.RegisterSuccess, true, Input.Email, user.Id,
                ipAddress: ip, userAgent: ua);

            return LocalRedirect(ReturnUrl);
        }
    }
}
