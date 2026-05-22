namespace UM_Project.Services.Interfaces
{
    public record IpGeoResult(string? Country, string? Region, string? City);

    public interface IIpGeoService
    {
        Task<IpGeoResult?> LookupAsync(string? ipAddress, CancellationToken cancellationToken = default);
    }
}
