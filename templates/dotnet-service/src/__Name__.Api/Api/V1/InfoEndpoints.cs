using TravelAssistant.__Name__.Services;
using TravelAssistant.BuildingBlocks.Hosting;

namespace TravelAssistant.__Name__.Api.V1;

/// <summary>
/// API layer: HTTP concerns only (routing, DTOs, validation, status codes). Calls a Services use case, never a repository.
/// Every endpoint lives under /api/v1/__name__/... so the gateway can route by prefix.
/// </summary>
public static class InfoEndpoints
{
    public static IEndpointRouteBuilder MapInfoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{ApiRoutes.V1}/__name__").WithTags("__Name__");

        group.MapGet("/info", (IGetServiceInfo useCase) =>
            {
                var descriptor = useCase.Execute();
                return TypedResults.Ok(new InfoResponse(descriptor.Name, descriptor.DisplayName, descriptor.Version, descriptor.Environment));
            })
            .WithName("Get__Name__Info")
            .WithSummary("Returns service name, version and environment.");

        return app;
    }
}

public sealed record InfoResponse(string Service, string DisplayName, string Version, string Environment);
