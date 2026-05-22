namespace UM_Project.Services.Interfaces
{
    public interface IAdminPasswordService
    {
        Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string newPassword, bool requireChangeOnNextLogin = true);
    }
}
