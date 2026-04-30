using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface IGeoTimeZoneClient
{
    Task<GeoTimeZoneLookupResult> LookupByIpAsync(string ipAddress, CancellationToken cancellationToken = default);
}
