using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface ITokenService
    {
        Task<AuthTokenResult> GenerateTokensAsync(ApplicationUser user, string? ipAddress);
        Task<AuthTokenResult?> RefreshTokensAsync(string refreshToken, string? ipAddress);
        Task RevokeUserTokensAsync(string userId, string reason);
        Task RevokeRefreshTokenAsync(string refreshToken, string reason);
    }
}
