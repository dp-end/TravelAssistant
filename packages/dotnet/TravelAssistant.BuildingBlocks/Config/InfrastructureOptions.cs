using System.ComponentModel.DataAnnotations;

namespace TravelAssistant.BuildingBlocks.Config;

// Env var mapping (":" becomes "__"): Redis__ConnectionString, Postgres__ConnectionString, RabbitMq__Uri

public sealed class RedisOptions
{
    public const string Section = "Redis";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}

public sealed class PostgresOptions
{
    public const string Section = "Postgres";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}

public sealed class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    [Required]
    public string Uri { get; init; } = string.Empty;
}
