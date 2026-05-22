using Microsoft.AspNetCore.Identity;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AdminPasswordService : IAdminPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminPasswordService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(
            string userId, string newPassword, bool requireChangeOnNextLogin = true)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

            user.MustChangePassword = requireChangeOnNextLogin;
            await _userManager.UpdateAsync(user);
            return (true, null);
        }
    }
}
