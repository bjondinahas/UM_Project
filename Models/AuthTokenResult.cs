namespace UM_Project.Models
{
    public class AuthTokenResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresUtc { get; set; }
        public DateTime RefreshTokenExpiresUtc { get; set; }
    }
}
