namespace TravelAssistant.__Name__.Config;

/// <summary>
/// Service specific settings. Section "__Name__" in appsettings.json, or env vars like __Name_____DisplayName.
/// Add [Required]/[Range] attributes to new properties: validation runs at startup (fail fast).
/// </summary>
public sealed class __Name__Options
{
    public const string Section = "__Name__";

    public string DisplayName { get; init; } = "__Name__ Service";
}
