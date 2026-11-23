using System.Net;
using System.Text.Json;

namespace TravelAssistant.__Name__.UnitTests;

public sealed class ErrorFormatTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ErrorFormatTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Unknown_route_returns_problem_details_with_correlation_id()
    {
        var response = await _client.GetAsync("/api/v1/__name__/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("correlationId").GetString()));
        Assert.True(json.RootElement.TryGetProperty("traceId", out _));
    }
}
