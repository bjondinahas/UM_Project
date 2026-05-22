using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface IAuthCookieService
    {
        void SetAuthCookies(HttpResponse response, AuthTokenResult tokens);
        void ClearAuthCookies(HttpResponse response);
        string? GetRefreshToken(HttpRequest request);
        string? GetAccessToken(HttpRequest request);
    }
}
