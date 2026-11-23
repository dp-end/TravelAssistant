namespace TravelAssistant.BuildingBlocks.Hosting;

/// <summary>Route constants shared by all services. Versioning is path based: /api/v1/...</summary>
public static class ApiRoutes
{
    public const string V1 = "/api/v1";
    public const string Health = "/health";
    public const string Ready = "/ready";
}
