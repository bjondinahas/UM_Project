namespace UM_Project.Models
{
    public class AuthAuditLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public static class AuthEventTypes
    {
        public const string LoginSuccess = "LoginSuccess";
        public const string LoginFailed = "LoginFailed";
        public const string Logout = "Logout";
        public const string RegisterSuccess = "RegisterSuccess";
        public const string RegisterFailed = "RegisterFailed";
        public const string TokenRefresh = "TokenRefresh";
        public const string TokenRefreshFailed = "TokenRefreshFailed";
        public const string TokenRevoked = "TokenRevoked";
        public const string AccountLocked = "AccountLocked";
    }
}
