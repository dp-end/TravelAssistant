using NetArchTest.Rules;

namespace TravelAssistant.__Name__.UnitTests;

/// <summary>
/// Enforces the layer rule from the "Layers" table in the service README.md:
///   Api -> Services -> Domain ; Repositories / Clients / Cache / Events are infrastructure used by Services only.
/// A violation fails CI, so the rule is more than documentation.
/// </summary>
public sealed class ArchitectureTests
{
    private const string Root = "TravelAssistant.__Name__";
    private static readonly System.Reflection.Assembly Assembly = typeof(Program).Assembly;

    [Fact]
    public void Api_must_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(Assembly)
            .That().ResideInNamespace($"{Root}.Api")
            .ShouldNot().HaveDependencyOnAny($"{Root}.Repositories", $"{Root}.Clients", $"{Root}.Cache", $"{Root}.Events")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Domain_must_not_depend_on_any_other_layer()
    {
        var result = Types.InAssembly(Assembly)
            .That().ResideInNamespace($"{Root}.Domain")
            .ShouldNot().HaveDependencyOnAny(
                $"{Root}.Api", $"{Root}.Services", $"{Root}.Repositories", $"{Root}.Clients",
                $"{Root}.Cache", $"{Root}.Events", "TravelAssistant.BuildingBlocks")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Services_must_not_depend_on_api()
    {
        var result = Types.InAssembly(Assembly)
            .That().ResideInNamespace($"{Root}.Services")
            .ShouldNot().HaveDependencyOn($"{Root}.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        "Violating types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
