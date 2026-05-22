using System.IdentityModel.Tokens.Jwt;
using UM_Project.Services;
using UM_Project.Services.Interfaces;

namespace UM_Project.Middleware
{
    /// <summary>
    /// Silently refreshes access token when expired but refresh token is still valid.
    /// </summary>
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRefreshMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService, IAuthCookieService cookieService, IAuthAuditService auditService)
        {
            var accessToken = cookieService.GetAccessToken(context.Request);
            var refreshToken = cookieService.GetRefreshToken(context.Request);

            if (!string.IsNullOrEmpty(accessToken) && IsTokenExpired(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var newTokens = await tokenService.RefreshTokensAsync(refreshToken, ip);
                if (newTokens != null)
                {
                    cookieService.SetAuthCookies(context.Response, newTokens);
                    await auditService.LogAsync(
                        Models.AuthEventTypes.TokenRefresh, true,
                        ipAddress: ip,
                        userAgent: context.Request.Headers.UserAgent);
                }
            }

            await _next(context);
        }

        private static bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}
