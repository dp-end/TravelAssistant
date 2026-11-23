using Microsoft.AspNetCore.Http.Features;
using TravelAssistant.BuildingBlocks.Correlation;

namespace TravelAssistant.BuildingBlocks.Errors;

/// <summary>
/// Standard error format for every service: RFC 9457 Problem Details (application/problem+json),
/// extended with correlationId and traceId so a client reported error can be found in the logs.
/// </summary>
public static class ProblemDetailsSetup
{
    public static IServiceCollection AddStandardProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var http = context.HttpContext;
                context.ProblemDetails.Instance ??= http.Request.Path;
                context.ProblemDetails.Extensions["correlationId"] = CorrelationId.Get(http);
                context.ProblemDetails.Extensions["traceId"] =
                    http.Features.Get<IHttpActivityFeature>()?.Activity?.Id ?? http.TraceIdentifier;
            };
        });
        return services;
    }

    public static IApplicationBuilder UseStandardErrorHandling(this IApplicationBuilder app)
    {
        // Unhandled exceptions -> 500 problem+json (exception details hidden outside Development).
        app.UseExceptionHandler();
        // Empty body error status codes (404, 405, 415...) -> problem+json as well.
        app.UseStatusCodePages();
        return app;
    }
}
