namespace UM_Project.Models
{
    public class CourseDocument
    {
        public int CourseDocumentId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string StoredPath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public Course Course { get; set; } = null!;
    }
}
