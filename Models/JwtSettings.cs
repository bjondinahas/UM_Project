namespace UM_Project.Models
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "UM_Project";
        public string Audience { get; set; } = "UM_Project";
        public int AccessTokenMinutes { get; set; } = 15;
        public int RefreshTokenDays { get; set; } = 7;
    }
}
