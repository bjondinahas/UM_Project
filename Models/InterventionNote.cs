namespace UM_Project.Models
{
    public class InterventionNote
    {
        public int InterventionNoteId { get; set; }
        public int StudentId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Student Student { get; set; } = null!;
    }
}
