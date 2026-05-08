using Microsoft.AspNetCore.Identity;

namespace UM_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string CustomId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
