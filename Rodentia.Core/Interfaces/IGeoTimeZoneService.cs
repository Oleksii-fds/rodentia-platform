using Rodentia.Core.Models;

namespace Rodentia.Core.Interfaces;

public interface IGeoTimeZoneService
{
    Task<GeoTimeZoneLookupResult> GetVisitorTimeZoneAsync(string ipAddress, CancellationToken cancellationToken = default);
}
