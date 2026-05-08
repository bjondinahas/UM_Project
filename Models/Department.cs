using System.ComponentModel.DataAnnotations;
namespace UM_Project.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        [Required]
        public string DepartmentName { get; set; } = string.Empty;
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Professor> Professors { get; set; } = new List<Professor>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
