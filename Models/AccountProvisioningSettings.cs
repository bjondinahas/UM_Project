namespace UM_Project.Models
{
    public class AccountProvisioningSettings
    {
        public string EmailDomain { get; set; } = "umproject.edu";
        public string DefaultPassword { get; set; } = "Welcome@123";
        public string StudentNumberPrefix { get; set; } = "STU";
        public string ProfessorIdPrefix { get; set; } = "PROF";
    }

    public class ProvisionedAccountResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string Email { get; set; } = string.Empty;
        public string GeneratedId { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
