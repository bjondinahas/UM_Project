using System.ComponentModel.DataAnnotations;

namespace UM_Project.Models
{
    public class StudentRegistrationViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Student full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Display(Name = "Add parent / guardian now")]
        public bool AddParent { get; set; }

        [Display(Name = "Parent full name")]
        public string? ParentFullName { get; set; }

        [EmailAddress]
        [Display(Name = "Parent email (optional)")]
        public string? ParentEmail { get; set; }
    }
}
