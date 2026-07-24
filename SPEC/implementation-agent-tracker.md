# SargentNexus Implementation Agent Tracker

## Purpose
Track implementation work that Copilot-driven implementation agents should complete, what is currently active, and what has already been finished.

## Current Status
- Current implementation slice: T011 Implement admin-issued temporary password reset as the P1 extension path.
- Current owner: auth implementation agent
- Current state: Ready
- Last updated: 2026-07-23

## Completed
- T001 Create the solution structure and project references.
- T002 Add the EF Core DbContext, migrations, and audit storage foundations.
- T003 Implement shared `/api/v1` routing conventions and problem-details error handling.
- T004 Implement API validation plumbing and OpenAPI scaffolding.
- T005 Implement login request handling with optional organization resolution.
- T006 Implement password hashing and verification.
- T007 Enforce password complexity, inactive-account denial, and 5-in-15 lockout rules.
- T008 Seed the global Site Admin from an environment-provided initial credential.
- T009 Implement forced first-login password change.
- T010 Emit audit events for authentication outcomes and password changes.
- T011 Implement admin-issued temporary password reset as the P1 extension path.

## In Progress
- None.

## Ready Next
- T012 Implement organization create, detail, list, edit, and archive flows.

## Progress Notes
- T001 completed: created `SargentNexus.sln`, `global.json`, and the five core projects under `src/`.
- T002 completed: added EF Core 8 SQL Server and design packages, created the first domain entity set, added `SargentNexusDbContext`, wired SQL Server registration, and created the initial EF migration under `src/SargentNexus.Infrastructure/Persistence/Migrations`.
- T003 completed: removed the default weather sample, added a shared `api/v1` controller base, added `GET /api/v1/health`, and wired ASP.NET Core problem-details services into the API host.
- Local fix applied during T002: repaired a malformed `appsettings.Development.json` file that blocked EF tooling from loading host configuration.
- T004 completed: added explicit 400/401/403/404 contract coverage across written contracts and OpenAPI specs, configured API validation problem-details responses, enabled Swagger generation in development, and refactored domain and infrastructure placeholders into named files and folders.
- T005 completed: added the application login flow, persistence lookup, login endpoint, and contract alignment for globally unique email credentials.
- T006 completed: replaced placeholder password comparison with a PBKDF2 password hasher that can hash and verify stored passwords.
- T007 completed: added failed-attempt tracking, 15-minute rolling lockout behavior after 5 failures, `429` lockout responses, and the corresponding EF migration for user lockout fields.
- T008 completed: added startup seeding for the global Site Admin using `Seed:SiteAdminPassword` from configuration and automatic database migration during startup initialization.
- T009 completed: added bearer-token-backed `auth/me` and `auth/change-password` flows, password policy enforcement, and clearing of `MustChangePassword` after a successful password change.
- Runtime validation note: startup seeding reached EF migration and attempted to open the LocalDB connection when run with a temporary `Seed__SiteAdminPassword`, but LocalDB stalled before full startup could be observed in this environment.
- Remaining auth placeholder: bearer tokens are still opaque in-memory session tokens rather than durable signed tokens.
- T010 completed: auth flows now persist audit events for login success, login failure, lockout-related failure, and password change success or failure using the existing `audit_events` table.
- T011 completed: added temporary password issuance, 24-hour expiry tracking, login consumption of valid temporary passwords, and the admin-issued `/api/v1/users/{userId}/temporary-password` endpoint.
- Project layout note: the solution and source tree were moved from `SPEC/` to the project root, and the relocated solution builds successfully from there.

## Backlog By Slice

### Foundation Agent
- T001 Create the solution structure and project references.
- T002 Add the EF Core DbContext, migrations, and audit storage foundations.
- T003 Implement shared `/api/v1` routing conventions and problem-details error handling.
- T004 Implement API validation plumbing and OpenAPI scaffolding.

### Auth Agent
- T005 Implement login request handling with optional organization resolution.
- T006 Implement password hashing and verification.
- T007 Enforce password complexity, inactive-account denial, and 5-in-15 lockout rules.
- T008 Seed the global Site Admin from an environment-provided initial credential.
- T009 Implement forced first-login password change.
- T010 Emit audit events for authentication outcomes and password changes.
- T011 Implement admin-issued temporary password reset as the P1 extension path.

### Tenant Administration Agent
- T012 Implement organization create, detail, list, edit, and archive flows.
- T013 Provision default statuses and one default board during organization creation.
- T014 Implement organization-scoped user create, detail, list, and edit flows.
- T015 Enforce one organization and one role for each non-Site Admin user.
- T016 Enforce globally unique email addresses.
- T017 Support `Active` and `Inactive` user states.
- T018 Prevent the last Org Admin from removing their own admin access or deactivating themselves.
- T019 Emit audit events for organization and user administration actions.

### Workflow Configuration Agent
- T020 Implement organization-scoped status create, update, list, and soft-delete flows.
- T021 Implement board create, update, list, and detail flows.
- T022 Enforce minimum two swimlanes per board.
- T023 Support board subsets of organization statuses.
- T024 Persist swimlane reorder immediately after drag-and-drop.

### Collaboration Agent
- T025 Implement idea create, detail, list, update, and status change flows.
- T026 Enforce title and description limits.
- T027 Implement default left-most-swimlane status assignment on create.
- T028 Implement tag autocomplete after 2 characters.
- T029 Implement tag normalization, uniqueness, and merge-on-concurrency behavior.
- T030 Implement organization-scoped email-based mention resolution for ideas and comments.
- T031 Implement comment create, edit, delete, and chronological retrieval flows.
- T032 Implement upvote toggle with one active upvote per user per idea.
- T033 Restrict upvote removal to the user who cast it.
- T034 Allow board-configured Users to move any idea on an eligible board.
- T035 Keep completed ideas editable and collaborative.
- T036 Emit audit events for idea creation, edits, status changes, comments, and upvotes.

### Events Agent
- T037 Emit notification events for idea mentions, comment mentions, idea comments, and status changes.
- T038 Persist canonical idea links on notification events.
- T039 Keep outbound email delivery explicitly deferred outside MVP.

### Client Agent
- T040 Build the login flow for globally unique email credentials.
- T041 Build first-login password change and inactive-account UI states.
- T042 Build the Admin section for organizations and users.
- T043 Build board and status administration workflows.
- T044 Build idea detail, tags, mentions, comments, and upvote workflows.
- T045 Reflect Site Admin, Org Admin, User, and Read Only boundaries in the UI.

### Hardening Agent
- T046 Align OpenAPI with the written contracts.
- T047 Implement unit tests from the test strategy.
- T048 Implement integration tests for auth, organization scope, and collaboration flows.
- T049 Implement contract tests for schema and problem-details error behavior.
- T050 Verify seed behavior, organization bootstrap, audit generation, and deferred-scope boundaries end-to-end.

## Notes For Next Agent
- This workspace started as specs only; implementation begins by scaffolding the .NET solution and project references.
- Keep this document updated whenever a task starts or completes.