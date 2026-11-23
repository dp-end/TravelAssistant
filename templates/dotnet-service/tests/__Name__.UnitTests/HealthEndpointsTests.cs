using System.Net;
using System.Text.Json;

namespace TravelAssistant.__Name__.UnitTests;

public sealed class HealthEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_returns_200_and_only_liveness_checks()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("__name__", json.RootElement.GetProperty("service").GetString());

        var checkNames = json.RootElement.GetProperty("checks").EnumerateArray()
            .Select(c => c.GetProperty("name").GetString()).ToList();
        Assert.Contains("self", checkNames);
        Assert.DoesNotContain("redis", checkNames);
        Assert.DoesNotContain("postgres", checkNames);
        Assert.DoesNotContain("rabbitmq", checkNames);
    }

    [Fact]
    public async Task Ready_is_mapped_and_returns_json()
    {
        // Dependencies are not running in unit tests: readiness must degrade to 503 with a readable body,
        // never to an exception or an empty response.
        var response = await _client.GetAsync("/ready");

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.TryGetProperty("checks", out _));
    }
}
