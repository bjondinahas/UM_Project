namespace UM_Project.Models
{
    public class Grade
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int Value { get; set; }
        public DateTime DateRecorded { get; set; } = DateTime.Now;
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}
