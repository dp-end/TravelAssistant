namespace TravelAssistant.BuildingBlocks.Config;

public static class OptionsExtensions
{
    /// <summary>
    /// Bind a config section (env vars use "__" as separator, e.g. Redis__ConnectionString),
    /// validate DataAnnotations and FAIL FAST at startup if required values are missing.
    /// </summary>
    public static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection services, string sectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
