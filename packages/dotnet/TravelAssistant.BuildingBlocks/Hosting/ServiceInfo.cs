using System.Reflection;

namespace TravelAssistant.BuildingBlocks.Hosting;

/// <summary>Identity of the running service: exposed on the info endpoint and attached to every log line.</summary>
public sealed record ServiceInfo(string Name, string Version, string Environment)
{
    public static ServiceInfo Create(string name, IHostEnvironment env)
    {
        // SERVICE_VERSION is injected at Docker build time (git SHA). Falls back to the assembly version locally.
        var version = System.Environment.GetEnvironmentVariable("SERVICE_VERSION")
            ?? Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0-local";
        return new ServiceInfo(name, version, env.EnvironmentName);
    }
}
