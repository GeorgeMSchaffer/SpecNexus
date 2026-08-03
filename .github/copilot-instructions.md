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

## NuGet Packages

You **may** install NuGet packages when needed to fix build errors or implement features — but **always check with the user first** before adding a new package dependency.

When proposing a new package:
- State the package name and version
- Explain why it's needed
- Wait for approval before running `dotnet add package`

## Session and Branch Lifecycle

Every session **must** merge its feature branch into `dev` before closing, in addition to opening a PR to `main`. The `dev` branch is the active integration branch where all feature work is combined for testing. Do not skip this step — leaving work only on the feature branch means the developer cannot test all features together.

### Task completion checklist

Complete these steps in order after verifying the build and tests pass on the feature branch:

**1. Open a PR from the feature branch to `main`**

```powershell
gh pr create --base main --title "<title>" --body "<body>"
```

**2. Discover the `dev` worktree path**

```powershell
git worktree list | Select-String dev
```

This will show a line like:
```
C:/code/BIDataDictionary/copilot-worktrees/SargentNexus/<worktree-name>  <hash> [dev]
```

Use that path in the steps below. If no worktree for `dev` exists, check out `dev` in a temporary location:

```powershell
git worktree add C:/tmp/sn-dev dev
```

**3. Merge the feature branch into `dev` in the `dev` worktree**

```powershell
Set-Location "C:/code/BIDataDictionary/copilot-worktrees/SargentNexus/<dev-worktree-name>"
git fetch origin
git merge origin/<feature-branch> --no-ff -m "Merge <feature-branch> into dev"
```

**4. Resolve merge conflicts (if any)**

- For **server-side code** (`src/SargentNexus.API`, `src/SargentNexus.Application`, `src/SargentNexus.Domain`, `src/SargentNexus.Infrastructure`, `tests/`) where `dev` has extended beyond the feature branch: prefer `dev`'s version (`--ours`).
- For **SPEC files** (`SPEC/`) and **client-only files** (`src/SargentNexus.Client/`) that the feature branch owns: take the feature branch version (`--theirs`).
- After resolving, stage and complete the merge:

```powershell
git add .
git merge --continue
```

**5. Push `dev` to `origin/dev`**

```powershell
git push origin dev
```

**6. Report the commit hash** of the merge commit on `dev` in your completion message.

> **Note:** If a `dev` worktree was created just for this merge (step 2 fallback), remove it after pushing:
> ```powershell
> git worktree remove C:/tmp/sn-dev
> ```

## General Guidelines

- Follow existing code style and patterns in the repo
- Prefer surgical, minimal changes over sweeping rewrites
- Do not modify test projects unless adding a new API endpoint that needs a stub
- Do not commit secrets or sensitive data
