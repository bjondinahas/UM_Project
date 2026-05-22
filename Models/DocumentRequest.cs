namespace UM_Project.Models
{
    public static class DocumentRequestTypes
    {
        public const string Transcript = "Transcript";
    }

    public static class DocumentRequestStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Completed = "Completed";
    }

    public class DocumentRequest
    {
        public int DocumentRequestId { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string RequestType { get; set; } = DocumentRequestTypes.Transcript;
        public string Status { get; set; } = DocumentRequestStatuses.Pending;
        public string? ParentNotes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAtUtc { get; set; }
        public string? ProcessedByUserId { get; set; }
        public Student Student { get; set; } = null!;
    }
}
