namespace UM_Project.Models
{
    public class StudentIndexRow
    {
        public Student Student { get; set; } = null!;
        public int EnrollmentCount { get; set; }
        public double? AverageGrade { get; set; }
        public int GradeCount { get; set; }
    }
}
