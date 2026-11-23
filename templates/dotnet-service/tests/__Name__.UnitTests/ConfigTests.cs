using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TravelAssistant.__Name__.Config;

namespace TravelAssistant.__Name__.UnitTests;

public sealed class ConfigTests
{
    [Fact]
    public void Service_options_bind_from_configuration()
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b.UseSetting("__Name__:DisplayName", "From Test"));

        var options = factory.Services.GetRequiredService<IOptions<__Name__Options>>().Value;

        Assert.Equal("From Test", options.DisplayName);
    }

    public static TheoryData<string> RequiredKeys => new(ApiFactory.RequiredSettingKeys);

    [Theory]
    [MemberData(nameof(RequiredKeys))]
    public void Startup_fails_fast_when_a_required_setting_is_missing(string key)
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b.UseSetting(key, string.Empty));

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains(nameof(OptionsValidationException), exception.ToString());
    }
}
