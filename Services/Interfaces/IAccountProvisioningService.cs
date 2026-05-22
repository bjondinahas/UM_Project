using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface IAccountProvisioningService
    {
        Task<string> GenerateStudentNumberAsync();
        Task<string> GenerateProfessorIdAsync();
        Task<string> GenerateUniqueEmailAsync(string fullName, string rolePrefix);
        Task<ProvisionedAccountResult> ProvisionStudentAsync(string fullName, int departmentId);
        Task<ProvisionedAccountResult> ProvisionProfessorAsync(string fullName, int departmentId, string? title = null);
        Task<ProvisionedAccountResult> LinkOrCreateParentAsync(string fullName, string? email, int studentId);
        Task<ProvisionedAccountResult> AssignChildToExistingParentAsync(int parentGuardianId, int studentId);
    }
}
