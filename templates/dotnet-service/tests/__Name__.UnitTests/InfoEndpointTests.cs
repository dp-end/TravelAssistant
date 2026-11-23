using System.Net;
using System.Net.Http.Json;

namespace TravelAssistant.__Name__.UnitTests;

public sealed class InfoEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public InfoEndpointTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Info_returns_service_identity()
    {
        var response = await _client.GetAsync("/api/v1/__name__/info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InfoBody>();
        Assert.NotNull(body);
        Assert.Equal("__name__", body.Service);
        Assert.Equal("Testing", body.Environment);
        Assert.False(string.IsNullOrWhiteSpace(body.Version));
    }

    [Fact]
    public async Task Response_carries_a_generated_correlation_id()
    {
        var response = await _client.GetAsync("/api/v1/__name__/info");

        var header = Assert.Single(response.Headers.GetValues("X-Correlation-Id"));
        Assert.False(string.IsNullOrWhiteSpace(header));
    }

    [Fact]
    public async Task Incoming_correlation_id_is_echoed_back()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/__name__/info");
        request.Headers.Add("X-Correlation-Id", "test-corr-123");

        var response = await _client.SendAsync(request);

        Assert.Equal("test-corr-123", Assert.Single(response.Headers.GetValues("X-Correlation-Id")));
    }

    [Fact]
    public async Task OpenApi_document_is_served()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/__name__/info", text);
    }

    private sealed record InfoBody(string Service, string DisplayName, string Version, string Environment);
}
