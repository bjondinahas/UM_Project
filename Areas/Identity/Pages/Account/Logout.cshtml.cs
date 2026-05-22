using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthAuditService _auditService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, IAuthAuditService auditService)
        {
            _signInManager = signInManager;
            _auditService = auditService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

            await _signInManager.SignOutAsync();

            await _auditService.LogAsync(AuthEventTypes.Logout, true, email, userId,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: Request.Headers.UserAgent.ToString());

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        public Task<IActionResult> OnGetAsync() => OnPostAsync();
    }
}
