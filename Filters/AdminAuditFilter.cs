using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Filters
{
    public class AdminAuditFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> AuditedControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Students", "Users", "Departments", "Professors", "Courses",
            "Enrollments", "Grades", "Schedules", "Parents"
        };

        private readonly IAdminAuditService _audit;

        public AdminAuditFilter(IAdminAuditService audit) => _audit = audit;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            if (executed.Exception != null) return;
            if (!IsAdmin(executed.HttpContext.User)) return;
            if (!string.Equals(context.HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)) return;

            var controller = context.RouteData.Values["controller"]?.ToString();
            if (controller == null || !AuditedControllers.Contains(controller)) return;

            var action = context.RouteData.Values["action"]?.ToString() ?? "";
            var adminAction = action switch
            {
                "Create" => AdminActions.Create,
                "Edit" => AdminActions.Update,
                "DeleteConfirmed" => AdminActions.Delete,
                _ => null
            };
            if (adminAction == null) return;

            if (executed.Result is not (RedirectToActionResult or RedirectResult or ViewResult)) return;

            var entityId = context.RouteData.Values["id"]?.ToString()
                ?? context.HttpContext.Request.Form["id"].FirstOrDefault()
                ?? context.HttpContext.Request.Form["StudentId"].FirstOrDefault()
                ?? context.HttpContext.Request.Form["CourseId"].FirstOrDefault();

            var details = string.Join(", ",
                context.ActionArguments.Select(kv => $"{kv.Key}={kv.Value}"));

            await _audit.LogAsync(executed.HttpContext, adminAction, controller, entityId, details);
        }

        private static bool IsAdmin(System.Security.Claims.ClaimsPrincipal user) =>
            user.IsInRole(RoleNames.SuperAdmin) || user.IsInRole(RoleNames.Admin);
    }
}
