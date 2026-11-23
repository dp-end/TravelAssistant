# __name__ service

> Generated from `templates/dotnet-service` by `scripts/new-dotnet-service.py`. Replace this intro with what the service owns.

## Responsibility

TODO: one paragraph. What this service owns, which data it is the source of truth for, what it must never do.

## Layers (dependency direction: `Api -> Services -> Domain`)

| Folder          | Layer          | May depend on                                   | Must not                                   |
|-----------------|----------------|-------------------------------------------------|--------------------------------------------|
| `Api/`          | HTTP           | `Services`, DTOs                                | call `Repositories`, `Cache`, `Clients`    |
| `Services/`     | Use cases      | `Domain`, `Repositories`, `Clients`, `Cache`, `Events` | reference `Api`                     |
| `Domain/`       | Entities/rules | nothing (pure C#)                               | reference any framework or other layer     |
| `Repositories/` | DB access      | `Domain`, Npgsql/EF                             | contain business rules                     |
| `Clients/`      | Other services / external APIs | `Domain` DTOs                   | be called from `Api`                       |
| `Cache/`        | Redis access   | `Domain`                                        | be called from `Api`                       |
| `Events/`       | RabbitMQ publish/consume | `Domain`, contracts                   | be called from `Api`                       |
| `Config/`       | Options        | DataAnnotations                                 |                                            |

`tests/__Name__.UnitTests/ArchitectureTests.cs` enforces this with NetArchTest; CI fails on violations.

## Endpoints

| Route                       | Purpose                                       |
|-----------------------------|-----------------------------------------------|
| `GET /health`               | liveness (process up, no dependencies)        |
| `GET /ready`                | readiness (Redis/Postgres/RabbitMQ reachable) |
| `GET /openapi/v1.json`      | OpenAPI contract                              |
| `GET /scalar`               | interactive API docs (Development only)       |
| `GET /api/v1/__name__/info` | name, version, environment                    |

## Run locally

```bash
dotnet run --project src/__Name__.Api          # http://localhost:__PORT__
dotnet test tests/__Name__.UnitTests           # fast, no infrastructure
TEST_INFRA=1 TEST_REDIS_URL=... dotnet test tests/__Name__.IntegrationTests
```

Configuration comes from `appsettings*.json` and environment variables (`Section__Key`). Required values are
validated at startup; a missing one stops the process with a clear error instead of failing at first request.
