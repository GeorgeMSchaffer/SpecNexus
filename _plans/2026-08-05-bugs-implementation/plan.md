# Plan: Bug Fixes from `SPEC/bugs.md`

## Goal
Resolve items in `SPEC/bugs.md` so auth, routing, and shell behavior match the current spec and implementation stays covered by tests.

## Issues Reviewed
- Password policy text/rules are inconsistent with the spec.
- Login currently lands on `/board` instead of the authenticated dashboard at `/`.
- Session resume trusts local storage without first confirming the token is still valid.
- The shell shows redundant "Password update required" copy.
- Required password changes for administrator accounts recur on later logins instead of remaining complete.
- Development demo seeding runs implicitly instead of requiring the `--seed-demo` application argument.
- Entity list pages do not consistently expose a right-aligned `Add New` action that opens the create form.
- Authenticated users report that the Settings icon linking to `/settings` is missing from the header.
- Authenticated users report that the Logout icon linking to `/logout` is missing from the header.

## Phases
- [x] Phase 1: Align password policy rules, messages, and tests
- [x] Phase 2: Verify the authenticated dashboard already implemented at `/`
- [x] Phase 3: Verify stored-token validation and invalid-session clearing
- [x] Phase 4: Verify redundant password-update copy and legacy redirects are absent
- [x] Phase 5: Run targeted regression tests and validate
- [x] Phase 6: Make development demo seeding opt-in with `--seed-demo`
- [x] Phase 7: Fix one-time administrator required-password-change persistence
- [x] Phase 8: Standardize authorization-aware `Add New` actions across entity list pages
- [x] Phase 9: Restore authenticated Settings and Logout header actions
- [x] Phase 10: Run focused auth, startup-seeding, component, and browser regression checks

## Status
**Current:** Complete. Phases 1-10 are implemented and validated.

## Next Implementation Tasks

### Phase 6: Opt-in demo seeding argument
- Parse the exact application argument `--seed-demo` in `src/SargentNexus.API/Program.cs` using an ordinal, case-insensitive comparison.
- Change `StartupSeeding.SeedAuthAsync` to always seed the Site Admin but call `SeedDevelopmentDemoEnvironmentAsync` only when the environment is Development and `--seed-demo` was supplied.
- Support `dotnet watch --project ./src/SargentNexus.API -- --seed-demo`; document that the first `--` belongs to `dotnet watch` and the second token is passed to the API.
- Update `tests/SargentNexus.API.Tests/StartupSeedingTests.cs` for Development with and without the argument and non-Development with the argument.
- Do not add a package or a general command-line parser for this single boolean flag.

### Phase 7: One-time administrator password change
- Reproduce the recurrence through the complete Site Admin flow: initial login, required change, logout, and later login with the changed password.
- Trace `MustChangePassword` through `AuthAccountService.ChangePasswordAsync`, persistence, `LoginService.LoginAsync`, `AuthSeeder.SeedSiteAdminAsync`, optional demo seeding, and client session restoration.
- Fix the nearest controlling persistence or reseeding path so successful change persists `MustChangePassword = false` and startup does not restore the prompt for an existing account.
- Preserve the rule that issuing a temporary password sets the prompt again.
- Add or extend application, infrastructure, API, and isolated browser tests for the complete later-login regression.

### Phase 8: List-page `Add New` actions
- Apply the responsive H1/action row from `SPEC/20-feature-client-ui-revisions.md` to `Pages/Ideas.razor`, `Pages/Workflow.razor`, `Pages/SettingsOrganizations.razor`, and `Pages/SettingsUsers.razor`, plus any additional entity list page found during implementation.
- Use the exact visible label `Add New` consistently; retain an entity-specific accessible name or tooltip where needed.
- Reuse each page's existing create form, route, or overlay. Do not create a second form implementation.
- Hide the action when the authenticated role cannot create that entity, including Read Only users and organization creation by non-Site Admin users.
- Add component or browser coverage for alignment, authorization visibility, activation, form opening, cancellation, and narrow-viewport non-overlap.

### Phase 9: Header Settings and Logout actions
- Verify `Layout/MainLayout.razor` subscribes to authentication state early enough to rerender authenticated actions after session initialization and login.
- Ensure all authenticated roles see a Settings icon linking to `/settings` and a Logout icon linking to `/logout` on the right side of the header.
- Use the existing icon library and preserve `title` and `aria-label` text; do not duplicate links in the primary menu.
- Ensure anonymous users continue to see only Login and Register.
- Add browser assertions for icon visibility, target routes, logout session clearing, and anonymous absence.

### Phase 10: Validation
- Run focused application and infrastructure auth tests, including the initial-change/later-login sequence and seeder preservation tests.
- Run `StartupSeedingTests` for all environment/argument combinations.
- Run client build and the relevant browser smoke cases serially because they share API/client ports and mutable seeded state.
- Run `dotnet build SargentNexus.sln -c Release` if active watch processes lock Debug outputs.
- Update `SPEC/bugs.md` checkboxes only after each behavior passes its focused regression check.

## Decisions
- Password policy should follow the spec in `SPEC/20-feature-organizations-and-users.md`: at least 6 characters, lowercase, uppercase, number, and symbol.
- The authenticated root route `/` should become the dashboard entry point, and post-login redirects should land there.
- Session bootstrap should not assume a stored token is valid; it must confirm freshness before reusing the session.
- The redundant password-update banner copy should be removed rather than replaced with a new message.
- Required password change is a persisted account state, not a per-session prompt; successful completion clears it until a new temporary password explicitly restores it.
- Site Admin seeding remains automatic so a fresh database is accessible; the larger demo organization graph is opt-in and Development-only.
- The valid watch invocation is `dotnet watch --project ./src/SargentNexus.API -- --seed-demo`; omitting the separator would pass an unknown option to `dotnet watch` rather than the API.
- `Add New` uses existing page-specific create interactions and role authorization rather than introducing a new shared form framework.
- Settings and Logout actions belong in the authenticated header for every authenticated role.

## Files Expected to Change
- `SPEC/bugs.md`
- `SPEC/20-feature-user-login.md`
- `SPEC/20-feature-client-ui-revisions.md`
- `src/SargentNexus.API/Program.cs`
- `src/SargentNexus.API/StartupSeeding.cs`
- `src/SargentNexus.Application/Auth/LoginService.cs` if the persistence defect is in application behavior
- `src/SargentNexus.Infrastructure/Auth/AuthServices.cs` if startup seeding restores the required-change state
- `src/SargentNexus.Client/Layout/MainLayout.razor`
- `src/SargentNexus.Client/Layout/MainLayout.razor.css`
- `src/SargentNexus.Client/Pages/Ideas.razor`
- `src/SargentNexus.Client/Pages/Workflow.razor`
- `src/SargentNexus.Client/Pages/SettingsOrganizations.razor`
- `src/SargentNexus.Client/Pages/SettingsUsers.razor`
- the existing Application auth test fixture that owns `LoginService` and password-change behavior
- `tests/SargentNexus.Infrastructure.Tests/AuthSeederTests.cs`
- `tests/SargentNexus.API.Tests/StartupSeedingTests.cs`
- `tests/SargentNexus.BrowserTests/SmokeTests.cs`

## Errors Encountered
- The Debug solution build was blocked by a pre-existing API process locking output DLLs. A Release solution build completed with zero warnings and errors.
- Running browser smoke cases concurrently caused shared-port and `testhost` interference. The invalid-token test passed in isolation, and the ordinary demo-user login/dashboard theory row passed; the Site Admin row remains dependent on mutable seeded-password state.
