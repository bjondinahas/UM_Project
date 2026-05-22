namespace UM_Project.Models
{
    public class AttendanceRecord
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int LessonSlotId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
        public int ProfessorId { get; set; }
        public DateTime MarkedAtUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public LessonSlot LessonSlot { get; set; } = null!;
        public Professor Professor { get; set; } = null!;
    }
}
