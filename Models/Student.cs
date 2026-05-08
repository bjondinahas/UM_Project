using System.ComponentModel.DataAnnotations;
namespace UM_Project.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Department Department { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
