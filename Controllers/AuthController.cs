using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthCookieService _cookieService;
        private readonly IAuthAuditService _auditService;

        public AuthController(ITokenService tokenService, IAuthCookieService cookieService, IAuthAuditService auditService)
        {
            _tokenService = tokenService;
            _cookieService = cookieService;
            _auditService = auditService;
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = _cookieService.GetRefreshToken(Request);
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _auditService.LogAsync(AuthEventTypes.TokenRefreshFailed, false,
                    failureReason: "Missing refresh token",
                    ipAddress: GetIp(), userAgent: GetUa());
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            var tokens = await _tokenService.RefreshTokensAsync(refreshToken, GetIp());
            if (tokens == null)
            {
                _cookieService.ClearAuthCookies(Response);
                await _auditService.LogAsync(AuthEventTypes.TokenRefreshFailed, false,
                    failureReason: "Invalid or expired refresh token",
                    ipAddress: GetIp(), userAgent: GetUa());
                return Unauthorized(new { message = "Session expired. Please log in again." });
            }

            _cookieService.SetAuthCookies(Response, tokens);
            await _auditService.LogAsync(AuthEventTypes.TokenRefresh, true,
                ipAddress: GetIp(), userAgent: GetUa());

            return Ok(new { message = "Token refreshed." });
        }

        private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
        private string? GetUa() => Request.Headers.UserAgent.ToString();
    }
}
