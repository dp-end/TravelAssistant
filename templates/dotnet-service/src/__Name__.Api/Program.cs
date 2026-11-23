using TravelAssistant.__Name__.Api.V1;
using TravelAssistant.__Name__.Config;
using TravelAssistant.__Name__.Services;
using TravelAssistant.BuildingBlocks.Config;
using TravelAssistant.BuildingBlocks.Health;
using TravelAssistant.BuildingBlocks.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Cross-cutting defaults: structured logging, correlation id, problem+json errors, OpenAPI, /health, /ready.
var info = builder.AddServiceDefaults("__name__");

// Service configuration (bound from appsettings + env vars, validated at startup).
builder.Services.AddValidatedOptions<__Name__Options>(__Name__Options.Section);

// Infrastructure clients + readiness checks (only the dependencies this service really uses).
builder.Services.AddRedis();                 // [redis]
builder.Services.AddPostgres();              // [postgres]
builder.Services.AddRabbitMq(info.Name);     // [rabbitmq]

// Application layer (use cases). Dependency rule: Api -> Services -> Domain. Api never touches Repositories/Cache/Clients.
builder.Services.AddScoped<IGetServiceInfo, GetServiceInfoHandler>();

var app = builder.Build();

app.MapServiceDefaults();
app.MapInfoEndpoints();

app.Run();

// Makes the implicit Program class visible to WebApplicationFactory<Program> in tests.
public partial class Program;
