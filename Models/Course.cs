using System.ComponentModel.DataAnnotations;

namespace UM_Project.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        [Required]
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int DepartmentId { get; set; }
        public int ProfessorId { get; set; }
        public Department Department { get; set; } = null!;
        public Professor Professor { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>(); 
    }
}
