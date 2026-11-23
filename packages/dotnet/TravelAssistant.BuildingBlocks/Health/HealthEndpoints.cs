using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TravelAssistant.BuildingBlocks.Hosting;

namespace TravelAssistant.BuildingBlocks.Health;

/// <summary>
/// /health = liveness: "is the process alive?" (no dependencies; Docker HEALTHCHECK uses this).
/// /ready  = readiness: "can it serve traffic?" (Redis/Postgres/RabbitMQ checks; the gateway and compose depends_on use this).
/// Both return JSON; status code is 200 when Healthy, 503 otherwise.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app, ServiceInfo info)
    {
        app.MapHealthChecks(ApiRoutes.Health, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = (context, report) => WriteAsync(context, report, info),
        });

        app.MapHealthChecks(ApiRoutes.Ready, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = (context, report) => WriteAsync(context, report, info),
        });

        return app;
    }

    private static Task WriteAsync(HttpContext context, HealthReport report, ServiceInfo info)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            service = info.Name,
            version = info.Version,
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message,
            }),
        };

        return context.Response.WriteAsJsonAsync(payload);
    }
}
