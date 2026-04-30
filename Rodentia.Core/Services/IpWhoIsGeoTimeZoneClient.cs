using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;

namespace Rodentia.Core.Services;

public sealed class IpWhoIsGeoTimeZoneClient : IGeoTimeZoneClient
{
    private readonly HttpClient _httpClient;

    public IpWhoIsGeoTimeZoneClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeoTimeZoneLookupResult> LookupByIpAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return CreateEmptyResult();

        var response = await _httpClient.GetFromJsonAsync<IpWhoIsResponse>(
            $"?ip={Uri.EscapeDataString(ipAddress)}",
            cancellationToken);

        if (response is null || !response.Success || string.IsNullOrWhiteSpace(response.TimeZoneId))
            return CreateEmptyResult();

        return new GeoTimeZoneLookupResult
        {
            TimeZoneId = response.TimeZoneId,
            LocalTime = response.LocalTime,
            City = response.City,
            Country = response.Country
        };
    }

    private static GeoTimeZoneLookupResult CreateEmptyResult() =>
        new()
        {
            TimeZoneId = string.Empty,
            LocalTime = DateTimeOffset.UtcNow,
            City = string.Empty,
            Country = string.Empty
        };

    private sealed class IpWhoIsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("timezone")]
        public string TimeZoneId { get; init; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; init; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; init; } = string.Empty;

        [JsonPropertyName("current_time")]
        public DateTimeOffset LocalTime { get; init; }
    }
}
