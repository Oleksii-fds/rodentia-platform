namespace Rodentia.Core.Models;

public sealed class GeoTimeZoneOptions
{
    public const string SectionName = "GeoTimeZone";

    public string BaseUrl { get; init; } = "https://ipwho.is/";

    public int TimeoutSeconds { get; init; } = 5;
}
