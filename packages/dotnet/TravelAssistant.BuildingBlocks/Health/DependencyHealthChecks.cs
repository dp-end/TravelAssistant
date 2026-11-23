using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TravelAssistant.BuildingBlocks.Config;
using Npgsql;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace TravelAssistant.BuildingBlocks.Health;

/// <summary>
/// Readiness checks for the three infrastructure dependencies. Each AddXxx method registers BOTH the client
/// (lazily created, nothing connects at startup) and a check tagged "ready". /health never touches them.
/// </summary>
public static class DependencyHealthChecks
{
    public static readonly string[] ReadyTag = ["ready"];
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        services.AddValidatedOptions<RedisOptions>(RedisOptions.Section);
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = sp.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString;
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false; // keep retrying in the background instead of throwing at first use
            options.ConnectTimeout = 3000;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", tags: ReadyTag, timeout: CheckTimeout);
        return services;
    }

    public static IServiceCollection AddPostgres(this IServiceCollection services)
    {
        services.AddValidatedOptions<PostgresOptions>(PostgresOptions.Section);
        services.AddSingleton(sp =>
            NpgsqlDataSource.Create(sp.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString));
        services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres", tags: ReadyTag, timeout: CheckTimeout);
        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, string clientName)
    {
        services.AddValidatedOptions<RabbitMqOptions>(RabbitMqOptions.Section);
        services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
        {
            Uri = new Uri(sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value.Uri),
            ClientProvidedName = clientName,
        });
        services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ReadyTag, timeout: CheckTimeout);
        return services;
    }
}

public sealed class RedisHealthCheck(IConnectionMultiplexer multiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await multiplexer.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"ping {latency.TotalMilliseconds:F1} ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("redis unreachable", ex);
        }
    }
}

public sealed class PostgresHealthCheck(NpgsqlDataSource dataSource) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = dataSource.CreateCommand("SELECT 1");
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("postgres unreachable", ex);
        }
    }
}

public sealed class RabbitMqHealthCheck(IConnectionFactory factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            return connection.IsOpen
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("rabbitmq connection not open");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("rabbitmq unreachable", ex);
        }
    }
}
