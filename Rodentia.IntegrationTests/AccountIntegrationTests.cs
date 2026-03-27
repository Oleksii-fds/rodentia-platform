using System.Net;
using Xunit;

namespace Rodentia.IntegrationTests;

public class AccountIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AccountIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Page_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/Account/Register");
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType!.ToString());
    }
}