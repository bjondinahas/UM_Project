namespace UM_Project.Models
{
    public class AdminActivityLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public static class AdminActions
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Download = "Download";
        public const string GradeOverride = "GradeOverride";
        public const string CalendarChange = "CalendarChange";
        public const string AtRiskNote = "AtRiskNote";
        public const string TranscriptDownload = "TranscriptDownload";
        public const string ReportExport = "ReportExport";
    }

    public static class AuditEntityTypes
    {
        public const string Transcript = "Transcript";
        public const string InterventionNote = "InterventionNote";
        public const string Calendar = "Calendar";
        public const string Grade = "Grade";
        public const string Report = "Report";
    }
}
