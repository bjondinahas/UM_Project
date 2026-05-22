namespace UM_Project.Models
{
    public class StudentDetailViewModel
    {
        public Student Student { get; set; } = null!;
        public ApplicationUser? LoginUser { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<ParentGuardian> Parents { get; set; } = new();
        public List<Schedule> Schedules { get; set; } = new();
        public double? AverageGrade { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public List<Course> AvailableCourses { get; set; } = new();
        public string DefaultPassword { get; set; } = "Welcome@123";
    }
}
