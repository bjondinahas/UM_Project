using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace UM_Project.Areas.Identity.Pages.Account
{
    [EnableRateLimiting("auth")]
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null) =>
            RedirectToPage("./Login", new { returnUrl });

        public IActionResult OnPost(string? returnUrl = null) =>
            RedirectToPage("./Login", new { returnUrl });
    }
}
