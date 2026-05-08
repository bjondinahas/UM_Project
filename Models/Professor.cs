namespace UM_Project.Models
{
    public class Professor
    {
        public int ProfessorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Department Department { get; set; } = null!;
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
