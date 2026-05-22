using System.Net;
using System.Text.Json;
using UM_Project.Helpers;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class IpGeoService : IIpGeoService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IpGeoService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<IpGeoResult?> LookupAsync(string? ipAddress, CancellationToken cancellationToken = default)
        {
            if (HttpContextExtensions.IsPrivateOrLocalIp(ipAddress))
                return new IpGeoResult("Local", "Local network", "Localhost");

            if (!IPAddress.TryParse(ipAddress, out _))
                return null;

            try
            {
                var client = _httpClientFactory.CreateClient("ipgeo");
                using var response = await client.GetAsync(
                    $"http://ip-api.com/json/{ipAddress}?fields=status,country,regionName,city",
                    cancellationToken);

                if (!response.IsSuccessStatusCode) return null;

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var root = doc.RootElement;
                if (root.GetProperty("status").GetString() != "success")
                    return null;

                return new IpGeoResult(
                    root.TryGetProperty("country", out var c) ? c.GetString() : null,
                    root.TryGetProperty("regionName", out var r) ? r.GetString() : null,
                    root.TryGetProperty("city", out var city) ? city.GetString() : null);
            }
            catch
            {
                return null;
            }
        }
    }
}
