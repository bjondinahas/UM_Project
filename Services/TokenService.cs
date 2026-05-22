using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwt;

        public TokenService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwt)
        {
            _db = db;
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthTokenResult> GenerateTokensAsync(ApplicationUser user, string? ipAddress)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var jwtId = Guid.NewGuid().ToString("N");
            var accessExpires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
            var refreshExpires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);

            var accessToken = BuildAccessToken(user, roles, jwtId, accessExpires);
            var refreshToken = GenerateSecureToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                JwtId = jwtId,
                ExpiresAtUtc = refreshExpires,
                CreatedByIp = ipAddress
            });
            await _db.SaveChangesAsync();

            return new AuthTokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresUtc = accessExpires,
                RefreshTokenExpiresUtc = refreshExpires
            };
        }

        public async Task<AuthTokenResult?> RefreshTokensAsync(string refreshToken, string? ipAddress)
        {
            var hash = HashToken(refreshToken);
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored == null || !stored.IsActive)
                return null;

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null)
                return null;

            stored.RevokedAtUtc = DateTime.UtcNow;
            stored.RevokedReason = "Replaced by new refresh token";

            var result = await GenerateTokensAsync(user, ipAddress);
            stored.ReplacedByTokenHash = HashToken(result.RefreshToken);
            await _db.SaveChangesAsync();

            return result;
        }

        public async Task RevokeUserTokensAsync(string userId, string reason)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
                token.RevokedReason = reason;
            }
            await _db.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, string reason)
        {
            var hash = HashToken(refreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (stored != null && stored.RevokedAtUtc == null)
            {
                stored.RevokedAtUtc = DateTime.UtcNow;
                stored.RevokedReason = reason;
                await _db.SaveChangesAsync();
            }
        }

        private string BuildAccessToken(ApplicationUser user, IList<string> roles, string jwtId, DateTime expires)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new("fullName", user.FullName)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}
