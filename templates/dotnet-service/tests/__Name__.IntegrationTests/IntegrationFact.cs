namespace TravelAssistant.__Name__.IntegrationTests;

/// <summary>
/// Integration tests need real infrastructure. They run only when the TEST_* connection env vars are present
/// (CI provides service containers; locally run `make test-integration` against the compose stack).
/// Without them the tests are SKIPPED, not failed, so `dotnet test` stays green on a fresh laptop.
/// </summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_INFRA")))
        {
            Skip = "Set TEST_INFRA=1 (plus TEST_REDIS_URL / TEST_POSTGRES_URL / TEST_RABBITMQ_URL) to run integration tests.";
        }
    }
}
