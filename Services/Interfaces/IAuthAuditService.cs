namespace UM_Project.Services.Interfaces
{
    public interface IAuthAuditService
    {
        Task LogAsync(string eventType, bool success, string? email = null, string? userId = null,
            string? failureReason = null, string? ipAddress = null, string? userAgent = null);
    }
}
