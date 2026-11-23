using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelAssistant.__Name__.IntegrationTests;

public sealed class ReadinessTests
{
    [IntegrationFact]
    public async Task Ready_reports_every_dependency_healthy()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.UseSetting("Redis:ConnectionString", Env("TEST_REDIS_URL"));       // [redis]
            b.UseSetting("Postgres:ConnectionString", Env("TEST_POSTGRES_URL")); // [postgres]
            b.UseSetting("RabbitMq:Uri", Env("TEST_RABBITMQ_URL"));              // [rabbitmq]
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/ready");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        using var json = JsonDocument.Parse(body);
        Assert.All(json.RootElement.GetProperty("checks").EnumerateArray(),
            check => Assert.Equal("Healthy", check.GetProperty("status").GetString()));
    }

    private static string Env(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"{name} is required for integration tests.");
}
