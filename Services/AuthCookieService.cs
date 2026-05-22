using Microsoft.Extensions.Options;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AuthCookieService : IAuthCookieService
    {
        public const string AccessTokenCookie = "um_access_token";
        public const string RefreshTokenCookie = "um_refresh_token";

        private readonly IWebHostEnvironment _env;

        public AuthCookieService(IWebHostEnvironment env) => _env = env;

        public void SetAuthCookies(HttpResponse response, AuthTokenResult tokens)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment() || true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Path = "/"
            };

            response.Cookies.Append(AccessTokenCookie, tokens.AccessToken, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                IsEssential = true,
                Path = "/",
                Expires = tokens.AccessTokenExpiresUtc
            });

            response.Cookies.Append(RefreshTokenCookie, tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = cookieOptions.HttpOnly,
                Secure = cookieOptions.Secure,
                SameSite = cookieOptions.SameSite,
                IsEssential = true,
                Path = "/",
                Expires = tokens.RefreshTokenExpiresUtc
            });
        }

        public void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenCookie);
            response.Cookies.Delete(RefreshTokenCookie);
        }

        public string? GetRefreshToken(HttpRequest request) =>
            request.Cookies.TryGetValue(RefreshTokenCookie, out var token) ? token : null;

        public string? GetAccessToken(HttpRequest request) =>
            request.Cookies.TryGetValue(AccessTokenCookie, out var token) ? token : null;
    }
}
