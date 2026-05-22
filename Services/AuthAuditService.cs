using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AuthAuditService : IAuthAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly IIpGeoService _geo;

        public AuthAuditService(ApplicationDbContext db, IIpGeoService geo)
        {
            _db = db;
            _geo = geo;
        }

        public async Task LogAsync(string eventType, bool success, string? email = null, string? userId = null,
            string? failureReason = null, string? ipAddress = null, string? userAgent = null)
        {
            var geo = await _geo.LookupAsync(ipAddress);

            _db.AuthAuditLogs.Add(new AuthAuditLog
            {
                EventType = eventType,
                Success = success,
                Email = email,
                UserId = userId,
                FailureReason = failureReason?.Length > 500 ? failureReason[..500] : failureReason,
                IpAddress = ipAddress,
                Country = geo?.Country,
                Region = geo?.Region,
                City = geo?.City,
                UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
