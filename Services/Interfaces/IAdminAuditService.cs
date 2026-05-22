namespace UM_Project.Services.Interfaces
{
    public interface IAdminAuditService
    {
        Task LogAsync(HttpContext httpContext, string action, string entityType, string? entityId = null, string? details = null);
    }
}
