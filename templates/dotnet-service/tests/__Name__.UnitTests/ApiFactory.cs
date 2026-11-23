using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelAssistant.__Name__.UnitTests;

/// <summary>
/// Boots the real service in-memory (no Kestrel, no Docker) with dummy dependency settings.
/// Dependencies are created lazily, so nothing connects unless a test hits /ready.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public static readonly string[] RequiredSettingKeys =
    [
        "Redis:ConnectionString",    // [redis]
        "Postgres:ConnectionString", // [postgres]
        "RabbitMq:Uri",              // [rabbitmq]
    ];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Redis:ConnectionString", "localhost:6379");                                             // [redis]
        builder.UseSetting("Postgres:ConnectionString", "Host=localhost;Username=test;Password=test;Database=test"); // [postgres]
        builder.UseSetting("RabbitMq:Uri", "amqp://guest:guest@localhost:5672/");                                   // [rabbitmq]
    }
}
