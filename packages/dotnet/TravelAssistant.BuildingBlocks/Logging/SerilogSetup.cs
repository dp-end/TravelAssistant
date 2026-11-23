using Serilog;
using Serilog.Formatting.Compact;
using TravelAssistant.BuildingBlocks.Hosting;

namespace TravelAssistant.BuildingBlocks.Logging;

public static class SerilogSetup
{
    /// <summary>
    /// Structured logging to stdout. JSON (CompactJsonFormatter) by default so Docker/CloudWatch can parse it.
    /// Set LOG_FORMAT=text for a human readable console while developing.
    /// </summary>
    public static void Configure(IHostBuilder host, ServiceInfo info)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Service", info.Name)
                .Enrich.WithProperty("Version", info.Version)
                .Enrich.WithProperty("Environment", info.Environment);

            var format = Environment.GetEnvironmentVariable("LOG_FORMAT");
            if (string.Equals(format, "text", StringComparison.OrdinalIgnoreCase))
            {
                configuration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Service} {CorrelationId} {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                configuration.WriteTo.Console(new CompactJsonFormatter());
            }
        });
    }
}
