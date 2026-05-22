namespace UM_Project.Helpers
{
    public static class HttpContextExtensions
    {
        public static string? GetClientIp(this HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();

            var ip = context.Connection.RemoteIpAddress;
            if (ip == null) return null;
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();
            return ip.ToString();
        }

        public static bool IsPrivateOrLocalIp(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return true;
            if (ip == "::1" || ip.StartsWith("127.") || ip.StartsWith("10.") ||
                ip.StartsWith("192.168.") || ip.StartsWith("172.16.") || ip.StartsWith("172.17.") ||
                ip.StartsWith("172.18.") || ip.StartsWith("172.19.") || ip.StartsWith("172.2") ||
                ip.StartsWith("172.30.") || ip.StartsWith("172.31.") || ip.StartsWith("169.254."))
                return true;
            return false;
        }
    }
}
