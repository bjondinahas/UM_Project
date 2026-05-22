using Microsoft.AspNetCore.Identity;

namespace UM_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string CustomId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        /// <summary>When true, user must set a new password before using the app.</summary>
        public bool MustChangePassword { get; set; }
    }
}
