namespace TravelAssistant.BuildingBlocks.Correlation;

/// <summary>Propagates the current correlation id to outgoing HttpClient calls (service-to-service).</summary>
public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = accessor.HttpContext;
        if (context is not null && !request.Headers.Contains(CorrelationId.HeaderName))
        {
            var id = CorrelationId.Get(context);
            if (!string.IsNullOrEmpty(id))
            {
                request.Headers.TryAddWithoutValidation(CorrelationId.HeaderName, id);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
