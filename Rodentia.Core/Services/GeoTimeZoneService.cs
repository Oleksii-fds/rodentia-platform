using Microsoft.Extensions.Logging;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public sealed class GeoTimeZoneService : IGeoTimeZoneService
{
    private readonly IGeoTimeZoneClient _geoTimeZoneClient;
    private readonly ILogger<GeoTimeZoneService> _logger;

    public GeoTimeZoneService(
        IGeoTimeZoneClient geoTimeZoneClient,
        ILogger<GeoTimeZoneService> logger)
    {
        _geoTimeZoneClient = geoTimeZoneClient;
        _logger = logger;
    }

    public async Task<GeoTimeZoneLookupResult> GetVisitorTimeZoneAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var byIp = await _geoTimeZoneClient.LookupByIpAsync(ipAddress, cancellationToken);
                if (!string.IsNullOrWhiteSpace(byIp.TimeZoneId))
                    return byIp;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve visitor timezone for IP: {IpAddress}", ipAddress);
        }

        return new GeoTimeZoneLookupResult
        {
            TimeZoneId = "UTC",
            LocalTime = DateTimeOffset.UtcNow,
            City = "Unknown",
            Country = "Unknown"
        };
    }
}
