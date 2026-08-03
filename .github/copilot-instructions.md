# Copilot Instructions — SargentNexus

SargentNexus is a spec-driven, organization-scoped collaboration and idea-tracking platform. Stack: .NET 8 (SDK pinned to `8.0.118` via `global.json`), ASP.NET Core API, Blazor client (Fluent UI Blazor), EF Core, xUnit.

## Architecture

Layered with strict boundaries — business rules live in Domain and Application, never in controllers or UI components.

| Project | Role | Depends on |
|---|---|---|
| `src/SargentNexus.API` | HTTP host, request boundary | Application |
| `src/SargentNexus.Application` | Use-case orchestration, authorization, validation | Domain |
| `src/SargentNexus.Domain` | Entities, enums, value objects, invariants | nothing (never API/Client/Infrastructure) |
| `src/SargentNexus.Infrastructure` | Persistence, external integrations | implements Application/Domain abstractions |
| `src/SargentNexus.Client` | Blazor UI (Fluent UI Blazor components) | — |
| `tests/` | Unit tests mirroring src layers, plus `SargentNexus.BrowserTests` (Playwright) | — |

## Spec governance — SPEC is the source of truth

- Read the relevant `SPEC/*.md` before implementing: `10-requirements.md` → requirements, `20-feature-*.md` → feature specs, `30-Contracts.md` → API contracts, `40-test-strategy.md` → test strategy, `90-definition-of-done.md` → definition of done.
- `SPEC/SPECKIT/` is derived. When behavior changes: update canonical `SPEC` docs first, then sync `SPECKIT`.
- Specs unclear or ambiguous? Ask before implementing. Implementation changed behavior? Update the spec.

Key domain rules (see `SPEC/20-feature-organizations-and-users.md`):
- Only Site Admin creates organizations (title + description + optional logo address); each org gets a system-generated invite code.
- Users join via invite-code self-registration, direct admin creation, or admin CSV import.

## Build, Test, Run

```powershell
dotnet build SargentNexus.sln                    # build all
dotnet test SargentNexus.sln                     # all tests
dotnet test tests/SargentNexus.Application.Tests/SargentNexus.Application.Tests.csproj   # one project (API.Tests / Infrastructure.Tests / BrowserTests likewise)
dotnet test tests/SargentNexus.Application.Tests/SargentNexus.Application.Tests.csproj --filter "FullyQualifiedName~ClassName.MethodName"
dotnet run --project .\src\SargentNexus.API\SargentNexus.API.csproj   # Swagger at http://localhost:5027/playground
```

**Port 5027 in use?** A previous `dotnet run`/`dotnet watch` is still alive. Stop it by PID, or use an alternate port: `$env:ASPNETCORE_URLS='http://localhost:5030'; dotnet run --project .\src\SargentNexus.API\SargentNexus.API.csproj`. Always stop API/watch processes you started before finishing a task.

## Coding standards

- Follow the [.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) and existing repo patterns; prefer surgical, minimal changes over rewrites.
- **NuGet:** never add a package without approval. Propose name + version + why, and wait before running `dotnet add package`.
- **EF Core:** all data access through DbContext; prefer LINQ over raw SQL; async/await for every database operation; schema changes via migrations.
- **SQL:** UPPERCASE keywords, lowercase table/column names, no `SELECT *`, meaningful aliases, consistent indentation.

## Testing

- Arrange/Act/Assert; names follow `Method_Scenario_ExpectedResult`.
- Cover happy path, boundary values, null input, and invalid state.
- Unit tests are hermetic: no network, file system, `DateTime.Now`, or randomness — inject fakes. EF Core tests use the InMemory provider, never a real database.
- Avoid duplicate setup; use builder/factory helpers.
- Integration coverage: API smoke tests in `SargentNexus.API.Tests`; end-to-end UI in `SargentNexus.BrowserTests` (Playwright).
- Do not modify test projects unless adding a new API endpoint that needs a stub.

## Workspace hygiene

- No temporary files, scratch code, or logs in the repo (`*.log`, `tmp-*.cs`, throwaway projects). Scratch work goes outside the repo; clean up before committing.
- Never commit secrets or sensitive data.
- Commit with clear, descriptive messages after a feature is implemented and tested.

## Session and branch lifecycle

Every session **must** merge its feature branch into `dev` (the active integration branch) before closing, in addition to opening a PR to `main`. Skipping this leaves work untestable alongside other features.

After build and tests pass on the feature branch:

1. **PR to main:** `gh pr create --base main --title "<title>" --body "<body>"`
2. **Find the dev worktree:** `git worktree list | Select-String dev` — if none exists, create one: `git worktree add C:/tmp/sn-dev dev`
3. **Merge into dev** (from the dev worktree):
   ```powershell
   git fetch origin
   git merge origin/<feature-branch> --no-ff -m "Merge <feature-branch> into dev"
   ```
4. **Conflicts:** for server-side code (`src/SargentNexus.{API,Application,Domain,Infrastructure}`, `tests/`) where `dev` moved ahead, prefer `dev` (`--ours`); for `SPEC/` and `src/SargentNexus.Client/` files the feature branch owns, take the feature branch (`--theirs`). Then `git add .` and `git merge --continue`.
5. **Push:** `git push origin dev`
6. **Report** the merge commit hash on `dev` in your completion message. Remove any temporary dev worktree (`git worktree remove C:/tmp/sn-dev`).
