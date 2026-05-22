using Microsoft.AspNetCore.Identity;
using UM_Project.Data;
using UM_Project.Helpers;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AdminAuditService : IAdminAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIpGeoService _geo;

        public AdminAuditService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IIpGeoService geo)
        {
            _db = db;
            _userManager = userManager;
            _geo = geo;
        }

        public async Task LogAsync(HttpContext httpContext, string action, string entityType, string? entityId = null, string? details = null)
        {
            var user = await _userManager.GetUserAsync(httpContext.User);
            var ip = httpContext.GetClientIp();
            var ua = httpContext.Request.Headers.UserAgent.ToString();
            var geo = await _geo.LookupAsync(ip);

            _db.AdminActivityLogs.Add(new AdminActivityLog
            {
                UserId = user?.Id,
                Email = user?.Email ?? httpContext.User.Identity?.Name,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details?.Length > 1000 ? details[..1000] : details,
                IpAddress = ip,
                Country = geo?.Country,
                Region = geo?.Region,
                City = geo?.City,
                UserAgent = ua.Length > 500 ? ua[..500] : ua,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
