<div align="center">

# ✈️ Travel Assistant

**An AI-powered, bilingual (Turkish / English) travel-planning assistant,
built as a set of containerised microservices.**

[![CI](https://github.com/dp-end/TravelAssistant/actions/workflows/ci.yml/badge.svg?branch=dev)](https://github.com/dp-end/TravelAssistant/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

Akdeniz University · Department of Computer Engineering · CSE 491 Graduation Project (Group 24)

</div>

---

## Table of contents

- [Status](#status)
- [Planned architecture](#planned-architecture)
- [Repository layout](#repository-layout)
- [Getting started](#getting-started)
- [Creating a new service](#creating-a-new-service)
- [Contributing](#contributing)
- [License](#license)

## Status

The project is in the **bootstrap phase**: we are building a solid, tested foundation first, so every
service we add later starts from the same quality bar.

| Area | State |
|------|-------|
| Shared build settings, pinned package versions | ✅ Done |
| Shared building blocks (logging, health, errors, correlation id) | ✅ Done |
| Service template + generator script | ✅ Done |
| Branch protection rules | ✅ Done |
| CI: build, unit tests, PR title check | ✅ Done |
| Docker Compose (PostgreSQL, Redis, RabbitMQ) | 🔜 Next |
| Integration tests in CI | 🔜 Planned |
| Real services (gateway, trip search, AI agent, vision) | 🔜 Planned |
| Web client (React) | 🔜 Planned |
| CD to AWS | 🔜 Planned |

## Planned architecture

| Concern | Technology |
|---------|------------|
| Backend services | .NET 10 (ASP.NET Core minimal APIs) |
| Vision service (landmark / MRZ) | Python |
| Data, cache, messaging | PostgreSQL, Redis, RabbitMQ |
| AI agent | DeepSeek LLM through Microsoft Agent Framework |
| Web client | React (React Native later) |
| Local environment | Docker Compose |
| Production | AWS EC2 |

Every .NET service follows the same layering, enforced by architecture tests
(a violation fails CI):

```
Api  ->  Services  ->  Domain
              |
              +-> Repositories / Clients / Cache / Events   (infrastructure, used by Services only)
```

## Repository layout

```
.
├── .github/
│   ├── workflows/        CI pipeline
│   └── rulesets/         Branch protection rules (importable JSON)
├── packages/dotnet/
│   └── TravelAssistant.BuildingBlocks/   Shared code every .NET service uses
├── templates/dotnet-service/             Template for a new service (API + tests)
├── scripts/
│   └── new-dotnet-service.py             Generates a service from the template
├── services/                             Created by the generator, one folder per service
├── Directory.Build.props                 Shared compiler settings (warnings are errors)
├── Directory.Packages.props              Every NuGet version, pinned in one place
├── global.json                           Pinned .NET SDK
└── TravelAssistant.slnx                  Solution
```

## Getting started

**Prerequisites**

- [.NET SDK](https://dotnet.microsoft.com/download) `10.0.200` or newer (see `global.json`)
- Python 3 (only for the service generator)
- Git

**Build and test**

```bash
git clone https://github.com/dp-end/TravelAssistant.git
cd TravelAssistant

dotnet build TravelAssistant.slnx
dotnet test <path-to-a-UnitTests-project>
```

The solution contains the shared building blocks and every service generated so far.
Right after cloning there are no services yet, so create one first (next section).

## Creating a new service

```bash
python scripts/new-dotnet-service.py <kebab-name> --port <dev-port> [--redis] [--postgres] [--rabbitmq]

# example
python scripts/new-dotnet-service.py trip --port 8087 --postgres --rabbitmq
```

The script copies the template to `services/<name>/`, keeps only the infrastructure you asked for
and adds the API, unit-test and integration-test projects to the solution.
Each generated service has `GET /health`, `GET /ready`, an OpenAPI document and an interactive
API reference in Development.

Afterwards: describe the service in its own `README.md`, then register it in Docker Compose
and the gateway routes once those exist.

## Contributing

We use a simple flow around two long-lived branches:

| Branch | Purpose |
|--------|---------|
| `main` | Production. Changes arrive only from `dev`. |
| `dev` | Integration branch, the default. Feature work is merged here. |

1. Create a branch from `dev`: `feat/<topic>`, `fix/<topic>`, `chore/<topic>`, `docs/<topic>`.
2. Commit with [Conventional Commits](https://www.conventionalcommits.org/) in English, for example
   `feat(trip): add search endpoint`.
3. Open a pull request into `dev`. The **PR title** must follow the same format.
4. CI must pass and **one teammate other than the author must approve**.
5. Merge into `dev` with **squash**. Releases from `dev` to `main` use a **merge commit**.

Direct pushes to `main` and `dev` are blocked. The exact rules live in
[`.github/rulesets/`](.github/rulesets) and can be re-imported from GitHub's
*Settings → Rules → Rulesets*.

## License

Released under the [MIT License](LICENSE).
