# SargentNexus Implementation Agent Tracker

## Purpose
Track implementation work that Copilot-driven implementation agents should complete, what is currently active, and what has already been finished.

## Current Status
- Current implementation slice: Epic 8 Hardening complete
- Current owner: main session
- Current state: Complete
- Last updated: 2026-08-03

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
- T051 Implement Development-only demo environment seed and startup gating.
- T052 Add unit and integration coverage for demo seed idempotency and dataset validation.
- T046 Align OpenAPI with the written contracts.
- T034 Allow board-configured Users to move any idea on an eligible board.
- T036 Emit audit events for idea creation, edits, status changes, comments, and upvotes.
- T048 Implement integration tests for auth, organization scope, and collaboration flows.
- O007 Add `InviteCode` and `InviteCodeGeneratedAtUtc` to `Organization` entity + EF migration.
- O008 Add `RegenerateInviteCodeAsync` endpoint (`POST /api/v1/organizations/{id}/invite-code/regenerate`).
- O009 Add self-registration endpoint (`POST /api/v1/auth/register`), `SelfRegistrationService`, and invite code generation on org create.
- O009-client Add invite code column to org list, Regenerate button in org form, `/register` self-registration page, and login page link.
- C1 Fix errant `else {` on ChangePassword.razor; delete Weather.razor and Counter.razor.
- C2 Rewrite MainLayout.razor with dark header `rgb(33,37,41)`, gear icon (→ `/settings`), sign-out icon, white username, horizontal nav (Home/Workflow/Ideas). NavMenu.razor removed.
- C5 Simplify Workflow.razor to boards-only list (Name, Board Type, Open).
- Domain-additions Add `Board.IsArchived` (bool) and `Status.Color/SortOrder/IsDefault` to domain + EF config + seed. Migration `AddBoardIsArchived` generated.
- T037–T039 Complete notification event wiring and coverage (idea/comment mentions, comment-added, status-changed), canonical `/ideas/{ideaId}/edit` links, self-notification suppression, and deferred-email DI guard coverage.
- T049 Add contract tests for 4xx problem-details response schemas, merged OpenAPI auth-register coverage, and integration validation of problem-details envelopes.
- T050 Verify seed behavior, organization bootstrap, audit generation, and deferred-scope boundaries end-to-end.

## In Progress
- none

## Ready Next
- none.

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
- T051 started: added Development-only startup hook and Infrastructure seeding implementation for 3 demo organizations, role users, board swimlane idea coverage, and example comments.
- T051 completed: startup now seeds demo data only in Development while always seeding Site Admin.
- T052 completed: added API startup-gating unit tests and expanded Infrastructure seed tests for per-organization graph validity plus reseed repair/idempotency invariants.
- T046 in progress: synced merged OpenAPI route inventory to include organization logo endpoint, corrected misplaced user update method in merged path fragments, and aligned idea/notification schemas with canonical contract fields.
- T046 completed: added merged OpenAPI response-schema coverage checks and contract drift tests, then aligned domain/infrastructure idea planning data fields (`priority`, `dueDate`, `assigneeUserId`) plus migration and seed compatibility updates.
- Collaboration baseline started: added `IdeasController` endpoints for idea list/create/detail/update/status move, comment list/create/edit/delete, and upvote toggle.
- Collaboration baseline started: expanded `IWorkflowManagementService` and `IWorkflowDataAccess` for idea/comment/upvote/tag/mention workflows and implemented end-to-end persistence-backed behavior in `WorkflowManagementService` and EF `WorkflowDataAccess`.
- Collaboration baseline quality gate: added API and Application tests for new ideas/upvote flows; API/Application/Infrastructure suites all pass.
- T028 completed: added `GET /api/v1/organizations/{organizationId}/tags` autocomplete endpoint with minimum-2-character validation, organization-scoped normalized-prefix matching, and limit support.
- T028 contract sync: updated canonical contracts and SPECKIT OpenAPI (feature and merged) with tag autocomplete route and problem-details response definitions.
- T028 test gate: added API controller tests, application service tests, and OpenAPI drift assertion for the new tags endpoint; API/Application suites pass.
- T034 completed: added board-level `allowUserStatusUpdate` configuration through domain, workflow models, and board create/update/detail/list responses.
- T034 authorization gate: `MoveIdeaStatusAsync` now permits User role status moves only when the board has `allowUserStatusUpdate=true`; Site Admin and Org Admin behavior is unchanged.
- T034 persistence and quality gate: added EF migration `AddBoardAllowUserStatusUpdate` and application tests covering both allowed and forbidden user move paths; API/Application/Infrastructure suites pass.
- T036 completed: workflow service now emits audit events for idea create/update/status move, comment create/update/delete, and upvote toggle actions.
- T036 infrastructure support: extended `IWorkflowAuditWriter` and `WorkflowAuditWriter` with persisted event types for each idea lifecycle action.
- T036 quality gate: expanded Application and Infrastructure tests to assert lifecycle audit emission; API/Application/Infrastructure suites pass.
- T047 progress: aligned collaboration role behavior to spec by allowing Read Only users to create/edit/delete their own comments and toggle upvotes while preserving Read Only denial for idea create/edit paths.
- T047 quality gate: added Read Only collaboration matrix unit tests in workflow service coverage; API/Application/Infrastructure suites pass.
- T047 progress: blocked unresolved idea mentions during save and added validation coverage for same-organization mention resolution.
- T047 quality gate: application tests now cover both failing and passing mention-resolution paths; API/Application/Infrastructure suites pass.
- T048 completed: added in-process API integration tests using `WebApplicationFactory` plus in-memory EF startup seeding to validate auth success/failure/lockout branches, unauthenticated protected-route rejection, and org-admin organization-scope collaboration read flows.
- T048 quality gate: API test suite passes with integration coverage included.
- T049 completed: added targeted API integration tests for unauthorized and validation problem-details envelopes plus OpenAPI drift assertions covering 4xx `application/problem+json` schemas across auth, administration, workflow, and collaboration paths.
- T049 contract sync: added missing self-registration route and schemas to the derived SPECKIT auth OpenAPI artifacts so merged OpenAPI matches the implemented `/api/v1/auth/register` endpoint.
- T050 completed: added end-to-end API coverage for Production startup seeding boundaries, site-admin organization bootstrap defaults, persisted audit events for login outcomes and organization creation, and merged OpenAPI assertions that deferred OAuth/OIDC/SAML endpoints remain absent from the MVP surface.
- Client slice progress: added Admin navigation entry for Site Admin/Org Admin plus new `/admin/organizations` client workflow for organization list/create/update/archive actions backed by the existing organization administration API.

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
- T051 Implement Development-only demo environment seed and startup gating.
- T052 Validate demo seed idempotency and seeded graph coverage with automated tests.

## Notes For Next Agent
- This workspace started as specs only; implementation begins by scaffolding the .NET solution and project references.
- Keep this document updated whenever a task starts or completes.