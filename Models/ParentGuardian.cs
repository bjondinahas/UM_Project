using System.ComponentModel.DataAnnotations;

namespace UM_Project.Models
{
    public class ParentGuardian
    {
        [Key]
        public int ParentId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}
