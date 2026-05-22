using Microsoft.AspNetCore.Identity;
using UM_Project.Models;

namespace UM_Project.Middleware
{
    public class MustChangePasswordMiddleware
    {
        private static readonly string[] AllowedPaths =
        {
            "/identity/account/changepassword",
            "/identity/account/logout",
            "/identity/account/login"
        };

        private readonly RequestDelegate _next;

        public MustChangePasswordMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                var allowed = AllowedPaths.Any(p => path.StartsWith(p, StringComparison.Ordinal));

                if (!allowed)
                {
                    var user = await userManager.GetUserAsync(context.User);
                    if (user?.MustChangePassword == true)
                    {
                        context.Response.Redirect("/Identity/Account/ChangePassword");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
