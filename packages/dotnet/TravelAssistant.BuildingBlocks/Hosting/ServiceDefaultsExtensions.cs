using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using TravelAssistant.BuildingBlocks.Correlation;
using TravelAssistant.BuildingBlocks.Errors;
using TravelAssistant.BuildingBlocks.Health;
using TravelAssistant.BuildingBlocks.Logging;

namespace TravelAssistant.BuildingBlocks.Hosting;

/// <summary>
/// One call per service for everything cross-cutting. Keeps Program.cs of each service tiny and identical in shape:
///   var info = builder.AddServiceDefaults("identity");  ... service specific registrations ...
///   app.MapServiceDefaults();                            ... service specific endpoints ...
/// </summary>
public static class ServiceDefaultsExtensions
{
    public static ServiceInfo AddServiceDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        var info = ServiceInfo.Create(serviceName, builder.Environment);
        builder.Services.AddSingleton(info);

        SerilogSetup.Configure(builder.Host, info);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
        builder.Services.AddStandardProblemDetails();
        builder.Services.AddOpenApi("v1");
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live", "ready"]);

        return info;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        // Order matters: correlation id first so every later log line (including error handling) carries it.
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseStandardErrorHandling();
        app.UseSerilogRequestLogging();

        var info = app.Services.GetRequiredService<ServiceInfo>();
        app.MapHealthEndpoints(info);

        // OpenAPI JSON is always on (it IS the service contract); the interactive UI only in Development.
        app.MapOpenApi();
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(); // served at /scalar
        }

        return app;
    }
}
