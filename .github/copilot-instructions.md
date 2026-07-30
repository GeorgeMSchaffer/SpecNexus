# Copilot Instructions — SargentNexus

SargentNexus is a spec-driven collaboration and project tracking platform (organization-scoped idea workflows). .NET 8 (SDK pinned to 8.0.118 via `global.json`), ASP.NET Core API, Blazor client, EF Core.

## Build, Test, Run

```powershell
# Build full solution
dotnet build SargentNexus.sln

# Run all tests
dotnet test SargentNexus.sln

# Run a single test project
dotnet test tests/SargentNexus.API.Tests/SargentNexus.API.Tests.csproj
dotnet test tests/SargentNexus.Application.Tests/SargentNexus.Application.Tests.csproj
dotnet test tests/SargentNexus.Infrastructure.Tests/SargentNexus.Infrastructure.Tests.csproj

# Run a single test or class
dotnet test tests/SargentNexus.Application.Tests/SargentNexus.Application.Tests.csproj --filter "FullyQualifiedName~ClassName.MethodName"

# Run the API (Swagger playground at http://localhost:5027/playground)
dotnet run --project .\src\SargentNexus.API\SargentNexus.API.csproj
```

### Port 5027 conflicts

If startup fails with "address already in use", a previous `dotnet run`/`dotnet watch` is still alive. Stop the specific process by PID, or run on an alternate port:

```powershell
$env:ASPNETCORE_URLS='http://localhost:5030'; dotnet run --project .\src\SargentNexus.API\SargentNexus.API.csproj
```

Always stop any API/watch processes you started before finishing a task.

## Architecture

Layered architecture with strict boundaries — business rules live in Domain and Application, never in controllers or UI components:

- `src/SargentNexus.API` — HTTP API host and request boundary; depends on Application.
- `src/SargentNexus.Application` — use-case orchestration, authorization, validation; depends on Domain.
- `src/SargentNexus.Domain` — entities, enums, value objects, invariants; must NOT depend on API, Client, or Infrastructure.
- `src/SargentNexus.Infrastructure` — persistence and external integrations; implements Application/Domain abstractions.
- `src/SargentNexus.Client` — Blazor UI (Fluent UI Blazor components).
- `tests/` — unit and boundary-focused test projects mirroring the src layers.

## Spec governance (SPEC is the source of truth)

- `SPEC/*.md` is the authoritative source for active MVP implementation decisions. Read the relevant spec before implementing:
  - Requirements: `SPEC/10-requirements.md`
  - Feature specs: `SPEC/20-feature-*.md`
  - API contracts: `SPEC/30-Contracts.md`
  - Test strategy: `SPEC/40-test-strategy.md`
  - Definition of done: `SPEC/90-definition-of-done.md`
- `SPEC/SPECKIT/` is a derived reference and review-tooling package. When behavior changes: update canonical `SPEC` docs first, then sync `SPECKIT` artifacts.
- When specs are unclear or ambiguous, ask for clarification before implementing.
- If implementation decisions change behavior, update the spec to reflect the new understanding.

## Coding standards

- Follow the .NET runtime coding style: https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md
- Do not add new NuGet packages without approval.

### SQL

- Uppercase SQL keywords (SELECT, FROM, WHERE); lowercase table and column names.
- No `SELECT *` — specify needed columns. Use meaningful aliases and consistent indentation.

### Entity Framework

- Use DbContext for database interactions; prefer LINQ over raw SQL.
- Use async/await for all database operations.
- Use migrations for schema changes.

### Testing

- Arrange/Act/Assert with naming: `Method_Scenario_ExpectedResult`.
- Cover happy path, boundary values, null input, and invalid state.
- EF Core tests use an InMemory database via DbContext — no real database connections.
- No network, file system, `DateTime.Now`, or randomness in unit tests; inject fakes.
- Separate smoke tests cover API endpoints for integration testing.
- Avoid duplicate setup; use builder/factory helpers.

## Workspace hygiene

- Never leave temporary files, scratch code, or logs in the repository (e.g., `*.log`, `tmp-*.cs`, throwaway test projects). Use a location outside the repo for scratch work and clean up before committing.
- Commit with clear, descriptive messages after a feature is implemented and tested.
