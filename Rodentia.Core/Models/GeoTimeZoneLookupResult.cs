namespace Rodentia.Core.Models;

public sealed class GeoTimeZoneLookupResult
{
    public required string TimeZoneId { get; init; }

    public required DateTimeOffset LocalTime { get; init; }

    public string City { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;
}
