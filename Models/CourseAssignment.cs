namespace UM_Project.Models
{
    public class CourseAssignment
    {
        public int CourseAssignmentId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public decimal WeightPercent { get; set; }
        public int? MaxPoints { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Course Course { get; set; } = null!;
    }
}
