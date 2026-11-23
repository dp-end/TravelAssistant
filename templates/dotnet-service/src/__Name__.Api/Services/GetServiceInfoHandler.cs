using Microsoft.Extensions.Options;
using TravelAssistant.__Name__.Config;
using TravelAssistant.__Name__.Domain;
using TravelAssistant.BuildingBlocks.Hosting;

namespace TravelAssistant.__Name__.Services;

/// <summary>Application layer: one interface + handler per use case. Orchestrates Domain, Repositories, Clients, Cache.</summary>
public interface IGetServiceInfo
{
    ServiceDetails Execute();
}

public sealed class GetServiceInfoHandler(ServiceInfo info, IOptions<__Name__Options> options) : IGetServiceInfo
{
    public ServiceDetails Execute() =>
        new(info.Name, options.Value.DisplayName, info.Version, info.Environment);
}
