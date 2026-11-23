using Serilog.Context;

namespace TravelAssistant.BuildingBlocks.Correlation;

/// <summary>
/// Reads X-Correlation-Id from the request (or creates one), stores it on the HttpContext,
/// echoes it on the response and pushes it into the Serilog LogContext so every log line carries it.
/// The gateway creates the id; downstream services only propagate it.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[CorrelationId.HeaderName].FirstOrDefault();
        var id = string.IsNullOrWhiteSpace(incoming) || incoming.Length > 128
            ? Guid.NewGuid().ToString("N")
            : incoming;

        context.Items[CorrelationId.ItemKey] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationId.HeaderName] = id;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await next(context);
        }
    }
}
