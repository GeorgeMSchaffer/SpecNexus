# SargentNexus

SargentNexus is a spec-driven proof of concept for a collaboration and project tracking platform focused on organization-scoped idea workflows.

## Most Important Project Goals

- Deliver an MVP where role boundaries and organization scoping are enforced consistently across API, application logic, and UI.
- Support end-to-end collaboration workflows for ideas: create, prioritize, discuss, mention, upvote, and move through board statuses.
- Keep implementation aligned to written contracts and acceptance criteria, with tests covering key business and boundary behaviors.
- Maintain layered architecture discipline so business rules remain in Domain and Application, not controllers or UI components.

## Specs To Know First

- Core requirements: [SPEC/10-requirements.md](SPEC/10-requirements.md)
- API contracts: [SPEC/30-Contracts.md](SPEC/30-Contracts.md)
- Test strategy: [SPEC/40-test-strategy.md](SPEC/40-test-strategy.md)
- Technical implementation plan: [SPEC/50-technical-implementation-plan.md](SPEC/50-technical-implementation-plan.md)
- Delivery sequencing: [SPEC/70-delivery-backlog.md](SPEC/70-delivery-backlog.md)
- Definition of done: [SPEC/90-definition-of-done.md](SPEC/90-definition-of-done.md)

## Spec Governance

- `SPEC/*.md` is the authoritative source for active MVP implementation decisions.
- `SPEC/SPECKIT` is a derived reference and review-tooling package.
- When behavior changes, update canonical `SPEC` docs first, then sync `SPECKIT` artifacts.

Additional context:

- Project brief: [SPEC/00-project-brief.md](SPEC/00-project-brief.md)
- Feature-level specs: All specs are labeled 20-{spec-name}.md such as:
[SPEC/20-feature-auth.md](SPEC/20-feature-auth.md), [SPEC/20-feature-organizations-and-users.md](SPEC/20-feature-organizations-and-users.md), [SPEC/20-feature-boards-and-statuses.md](SPEC/20-feature-boards-and-statuses.md), [SPEC/20-feature-ideas-and-engagement.md](SPEC/20-feature-ideas-and-engagement.md), [SPEC/20-feature-notifications.md](SPEC/20-feature-notifications.md)
- Team implementation standards: [.github/copilot-instructions.md](.github/copilot-instructions.md)

## Solution Structure

- src/SargentNexus.API: HTTP API host and request boundary.
- src/SargentNexus.Application: use-case orchestration, authorization, validation, and application rules.
- src/SargentNexus.Domain: entities, enums, value objects, and domain invariants.
- src/SargentNexus.Infrastructure: persistence and external integration implementations.
- src/SargentNexus.Client: Blazor UI.
- tests/: unit and boundary-focused test projects.

## Development Guidelines (Quick Reference)

- Clarify ambiguous requirements before implementing and update specs when decisions change behavior.
- Keep architecture boundaries strict:
	- API depends on Application.
	- Application depends on Domain.
	- Infrastructure implements Application/Domain abstractions.
	- Domain must not depend on API, Client, or Infrastructure.
- Follow .NET coding style guidance: https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md
- Follow SQL standards from [.github/copilot-instructions.md](.github/copilot-instructions.md): uppercase SQL keywords, lowercase table/column names, explicit column selection, and readable formatting.
- Follow test standards from [.github/copilot-instructions.md](.github/copilot-instructions.md): Arrange/Act/Assert, clear test naming, boundary and invalid-state coverage, and isolated tests without external dependencies.
- Do not add new packages without approval.

## Build

Build the full solution:

```powershell
dotnet build SargentNexus.sln
```

## API Playground

Developers can interact with the API using Swagger UI at:

- http://localhost:5027/playground

The playground is enabled by Development settings in [src/SargentNexus.API/appsettings.Development.json](src/SargentNexus.API/appsettings.Development.json).

### Using Authenticated Endpoints

1. Authenticate through the API (for example via the login endpoint) and copy the access token.
2. Open the playground URL.
3. Select Authorize and enter: `Bearer YOUR_ACCESS_TOKEN`
4. Use Try it out to execute secured requests.

### Port Already In Use

If startup fails with address already in use:

- Stop any existing `dotnet watch` or `dotnet run` process using port 5027.
- Or run on a different port:

```powershell
$env:ASPNETCORE_URLS='http://localhost:5030'; dotnet run --project .\src\SargentNexus.API\SargentNexus.API.csproj
```

## Test Projects

- tests/SargentNexus.API.Tests
- tests/SargentNexus.Application.Tests
- tests/SargentNexus.Infrastructure.Tests

## Run Tests

Run all tests in the solution:

```powershell
dotnet test SargentNexus.sln
```

Run a single test project:

```powershell
dotnet test tests/SargentNexus.API.Tests/SargentNexus.API.Tests.csproj
dotnet test tests/SargentNexus.Application.Tests/SargentNexus.Application.Tests.csproj
dotnet test tests/SargentNexus.Infrastructure.Tests/SargentNexus.Infrastructure.Tests.csproj
```
