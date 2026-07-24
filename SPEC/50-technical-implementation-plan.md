# Technical Implementation Plan

## Goal
Deliver the SargentNexus MVP as a layered ASP.NET Core application with a Blazor client, SQL Server persistence, organization-scoped authorization, audit coverage, and contract-aligned API behavior.

## Scope Alignment
- MVP in scope: authentication, organization and user management, boards, statuses, ideas, tags, comments, upvotes, mentions, audit events, and notification event definitions.
- Later phase: queued email notification delivery.
- Out of scope: OAuth/SSO, reporting, MFA, social login, and remember-this-device.
- P1 follow-up: admin-issued temporary password reset.

## Delivery Objective
Produce an implementation plan that is detailed enough to drive engineering execution with minimal interpretation by defining:
- the ownership of responsibilities across solution layers
- the persistence and contract strategy
- the build order of feature slices
- the validation gates that confirm spec compliance before moving forward
- the client composition needed for the documented workflows

## Architecture Plan
### Solution Layers
- `SargentNexus.Domain`: entities, enums, value objects, and invariants.
- `SargentNexus.Application`: use cases, authorization rules, validation, audit orchestration, and tenant-aware workflows.
- `SargentNexus.Infrastructure`: EF Core persistence, seeding, auditing persistence, and later-phase notification delivery integrations.
- `SargentNexus.API`: HTTP endpoints, request/response mapping, auth middleware, contract validation, and OpenAPI.
- `SargentNexus.Client`: Blazor UI, forms, boards, and collaboration interactions.

### Layer Responsibilities
#### Domain
- Own entity state and invariant enforcement.
- Model `Organization`, `User`, `Status`, `Board`, `BoardSwimlane`, `Idea`, `Tag`, `IdeaTag`, `Mention`, `Comment`, `Upvote`, `AuditEvent`, and `NotificationEvent`.
- Protect business invariants such as:
  - non-Site Admin users always belonging to exactly one organization
  - one active upvote per user per idea
  - minimum two swimlanes per board
  - soft-delete semantics for statuses

#### Application
- Orchestrate feature workflows and authorization checks.
- Resolve organization scope from caller identity and resource context.
- Coordinate login, organization bootstrap, tag normalization, mention resolution, comment permissions, upvote toggling, audit emission, and notification event creation.
- Define ports for persistence, password hashing, token generation, environment configuration, and event persistence.

#### Infrastructure
- Implement EF Core mappings, migrations, repositories or persistence adapters, and transaction boundaries.
- Provide concrete password hashing, token issuance, and seed credential loading.
- Persist audit and notification events.
- Keep later-phase email delivery behind an interface so MVP does not depend on outbound integration.

#### API
- Expose `/api/v1` endpoints matching `SPEC/30-Contracts.md`.
- Validate request shape and field constraints.
- Return problem-details-style error responses for non-2xx outcomes.
- Keep HTTP endpoints thin by delegating all business decisions to Application and Domain services.

#### Client
- Present role-aware workflows using Fluent UI-based pages and components.
- Handle login, admin workflows, board collaboration flows, and detail editing.
- Surface validation clearly without duplicating server-only business rules.

### Core Technical Decisions
- Enforce organization scoping in the Application layer, not in controllers or UI.
- Use a single SQL Server database with organization-owned rows, foreign keys, and a global Site Admin account outside tenant ownership.
- Keep API contracts versioned under `/api/v1` and aligned with `SPEC/30-Contracts.md`.
- Use problem-details-style error responses for all non-2xx outcomes.
- Validate request shape and field constraints at the API boundary; enforce business rules in Application and Domain layers.
- Use last-write-wins updates in MVP unless a narrower contract explicitly says otherwise.
- Treat audit generation as a first-class application concern for auth, administration, and idea lifecycle events.
- Model notification triggers now, but defer guaranteed email delivery infrastructure to a later phase.

### Runtime Architecture
- Authentication is API-driven with a current-user endpoint and a first-login password-change flag.
- Organization scope is applied centrally to all tenant-owned resource operations.
- Audit events are written during successful business transactions where applicable and during both successful and failed auth outcomes.
- Notification events are persisted as internal records during collaboration workflows without requiring outbound email dispatch.

## Implementation Slices
### Slice 1: Auth and Identity
- login, password verification, lockout behavior, inactive-account denial, current-user retrieval, first-login password change

### Slice 2: Tenant Administration
- organization create and archive flows, automatic default board and status provisioning, organization-scoped user CRUD, lifecycle state handling, last-Org-Admin safeguards

### Slice 3: Workflow Configuration
- statuses, boards, swimlane ordering, soft-delete status behavior, board status subset selection

### Slice 4: Idea Collaboration
- idea CRUD, tags, mentions, comments, upvotes, board-configured user moves, completed-idea behavior

### Slice 5: Cross-Cutting Eventing
- audit events, notification events, event payloads, future extension seams for event queries or delivery

## Detailed Delivery Phases
### Phase 1: Foundation
1. Create the solution structure and project references.
2. Establish shared primitives:
	- GUID identifier conventions
	- UTC timestamp handling
	- enums for roles and lifecycle states
	- reusable problem-details response mapping
3. Add EF Core DbContext, migrations, audit storage design, and base auditing fields.
4. Implement API conventions for `/api/v1`, problem-details handling, and request validation.
5. Set up authentication middleware, authorization policy registration, and current-user context resolution.
6. Seed the global Site Admin using an environment-provided initial credential and mark it for forced password change.

Validation gate:
- application boots successfully
- migrations apply cleanly
- non-2xx responses use the standard problem-details envelope
- protected endpoints reject anonymous callers

### Phase 2: Authentication and Access
1. Implement login with globally unique email credentials.
2. Enforce password complexity policy.
3. Enforce inactive-user denial.
4. Implement 5 failed attempts within 15 minutes causing a 15-minute lockout.
5. Implement first-login password change for the seeded Site Admin.
6. Add `GET /api/v1/auth/me` for client bootstrap.
7. Implement the later-phase admin-issued temporary password reset path without requiring self-service email reset.
8. Emit audit events for successful login, failed login, password change, and temporary password issuance.

Validation gate:
- login returns the authenticated payload for valid credentials
- invalid credentials, inactive accounts, and lockout behavior match the contract
- first-login password change is enforced before protected application use

### Phase 3: Organizations and Users
1. Implement organization create, detail, list, edit, and archive flows.
2. Hide archived organizations from default list results unless explicitly filtered in.
3. Automatically provision default statuses and one default board when a new organization is created.
4. Implement user CRUD with exactly one organization and one role for each non-Site Admin user.
5. Support `Active` and `Inactive` user states only.
6. Enforce globally unique email addresses.
7. Trim organization and user text fields before validation and persistence.
8. Enforce organization and user field maximum lengths from the contracts document.
9. Prevent the last Org Admin from removing their own admin access or deactivating themselves.
10. Emit audit events for organization changes, user changes, role changes, and account status changes.

Validation gate:
- Site Admin and Org Admin permissions match the rules matrix
- organization bootstrap creates the expected default board and status set
- archived organizations remain retained but excluded by default from list queries

### Phase 4: Boards and Statuses
1. Implement organization-scoped statuses with default provisioning.
2. Implement soft-delete status behavior while preserving existing idea and board references.
3. Implement boards with selectable subsets of organization statuses.
4. Enforce a minimum of two swimlanes per board.
5. Represent swimlane ordering explicitly through an ordered mapping entity.
6. Persist swimlane order immediately on drag-and-drop completion.
7. Preserve prior status names in detail and historical contexts with an archived or deleted label.

Validation gate:
- default statuses exist after organization creation
- boards reject fewer than two swimlanes
- status deletion does not orphan related ideas or board history
- swimlane reorder persists immediately and survives reload

### Phase 5: Ideas and Engagement
1. Implement idea create, detail, list, edit, and status update flows.
2. Enforce title maximum length of 150 characters and description maximum length of 4000 characters.
3. Default idea status to the left-most board swimlane when omitted.
4. Implement tag autocomplete after 2 characters.
5. Implement tag normalization:
	- trim whitespace
	- compare case-insensitively
	- enforce organization-scoped uniqueness
	- limit tag values to 100 characters
6. Merge concurrent creation of the same normalized tag into one stored tag.
7. Resolve mentions by same-organization email lookup for both ideas and comments.
8. Implement comments with chronological display, author edit and delete, admin delete in scope, and plain-text bodies up to 2000 characters.
9. Implement upvote toggle with one active upvote per user per idea and user-only removal.
10. Support board-configured User status changes for any idea on the board.
11. Keep completed ideas fully collaborative.
12. Emit audit events for idea creation, edits, status changes, comments, and upvote toggles.

Validation gate:
- collaboration permissions match the feature specs
- same-organization mention resolution works in ideas and comments
- duplicate concurrent tags converge to one normalized stored tag
- completed ideas remain editable and interactive

### Phase 6: Notifications and Audit Readiness
1. Emit notification events for idea mentions, comment mentions, comments on ideas, and idea status changes.
2. Persist canonical idea links using `/org/{organizationId}/boards/{boardId}/ideas/{ideaId}`.
3. Include both a human-readable message and structured metadata in audit events.
4. Persist audit and notification events for internal verification and processing only.
5. Do not expose event query endpoints in MVP unless scope changes.
6. Keep outbound email delivery explicitly deferred outside MVP.

Validation gate:
- required auth, admin, and idea lifecycle actions emit audit events
- collaboration triggers emit notification events
- event persistence works without an outbound notification subsystem

### Phase 7: Client Experience
1. Build the login flow for globally unique email credentials.
2. Build first-login password change and inactive-account handling states.
3. Build organization and user administration screens within a dedicated Admin section.
4. Build board and status administration screens, including immediate swimlane reorder persistence.
5. Build idea detail and board workflows with tags, email-based mentions, comments, and upvotes.
6. Reflect role boundaries clearly for Site Admin, Org Admin, User, and Read Only users.
7. Surface archived organizations, inactive users, and deleted-status historical references clearly but safely.

Implementation guidance:
- use Fluent UI command bars, panels, dialog patterns, details lists, form layouts, and cards consistently
- prefer clear labels, large action targets, inline helper text, and visible validation summaries
- keep admin surfaces separate from collaboration surfaces to reduce accidental misuse

Validation gate:
- core screens match documented user flows
- UI makes invalid actions difficult without replacing server enforcement
- role-based affordances are visible and understandable

### Phase 8: Hardening and Release
1. Align OpenAPI documents with every implemented `/api/v1` endpoint and the problem-details error contract.
2. Cover acceptance criteria with targeted unit, integration, contract, and end-to-end tests from `SPEC/40-test-strategy.md`.
3. Verify seed data, role boundaries, organization scoping, audit generation, notification event generation, and default organization bootstrap end-to-end.
4. Confirm deferred items stay deferred: OAuth, reporting, guaranteed email delivery, remember-this-device, and event query endpoints.

Validation gate:
- contracts and OpenAPI remain synchronized
- acceptance criteria are traceable to executable tests
- deferred scope does not leak into the release

## Data Model Outline
- Organization
- User
- Role
- Board
- BoardSwimlane or swimlane mapping
- Status
- Idea
- Tag
- IdeaTag
- Mention
- Comment
- Upvote
- AuditEvent
- NotificationEvent

## Persistence Design
### Core Tables or Aggregates
- `organizations`
- `users`
- `statuses`
- `boards`
- `board_swimlanes`
- `ideas`
- `tags`
- `idea_tags`
- `mentions`
- `comments`
- `upvotes`
- `audit_events`
- `notification_events`

### Table Outline
#### `organizations`
- `organization_id` uniqueidentifier primary key
- `company_name` nvarchar(200)
- `address` nvarchar(200)
- `city` nvarchar(100)
- `state` nvarchar(50)
- `zip` nvarchar(20)
- `phone` nvarchar(25)
- `primary_contact_first_name` nvarchar(100)
- `primary_contact_last_name` nvarchar(100)
- `is_archived` bit
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `users`
- `user_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier nullable for Site Admin only
- `first_name` nvarchar(100)
- `last_name` nvarchar(100)
- `email` nvarchar(320)
- `normalized_email` nvarchar(320)
- `password_hash` nvarchar(max)
- `role` nvarchar(50)
- `status` nvarchar(20)
- `must_change_password` bit
- `failed_login_count` int
- `lockout_window_start_utc` datetime2 nullable
- `locked_until_utc` datetime2 nullable
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `statuses`
- `status_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `name` nvarchar(100)
- `is_deleted` bit
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `boards`
- `board_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `name` nvarchar(150)
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `board_swimlanes`
- `board_id` uniqueidentifier foreign key
- `status_id` uniqueidentifier foreign key
- `display_order` int

#### `ideas`
- `idea_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `board_id` uniqueidentifier foreign key
- `author_user_id` uniqueidentifier foreign key
- `title` nvarchar(150)
- `description` nvarchar(4000)
- `status_id` uniqueidentifier foreign key
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `tags`
- `tag_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `name` nvarchar(100)
- `normalized_name` nvarchar(100)
- `created_at_utc` datetime2

#### `idea_tags`
- `idea_id` uniqueidentifier foreign key
- `tag_id` uniqueidentifier foreign key

#### `mentions`
- `mention_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `idea_id` uniqueidentifier nullable
- `comment_id` uniqueidentifier nullable
- `mentioned_user_id` uniqueidentifier foreign key
- `source_text` nvarchar(320)
- `created_at_utc` datetime2

#### `comments`
- `comment_id` uniqueidentifier primary key
- `idea_id` uniqueidentifier foreign key
- `author_user_id` uniqueidentifier foreign key
- `body` nvarchar(2000)
- `created_at_utc` datetime2
- `updated_at_utc` datetime2

#### `upvotes`
- `idea_id` uniqueidentifier foreign key
- `user_id` uniqueidentifier foreign key
- `created_at_utc` datetime2

#### `audit_events`
- `audit_event_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier nullable
- `actor_user_id` uniqueidentifier nullable
- `event_type` nvarchar(100)
- `entity_type` nvarchar(100)
- `entity_id` uniqueidentifier nullable
- `message` nvarchar(500)
- `metadata_json` nvarchar(max)
- `occurred_at_utc` datetime2

#### `notification_events`
- `notification_event_id` uniqueidentifier primary key
- `organization_id` uniqueidentifier foreign key
- `board_id` uniqueidentifier foreign key
- `idea_id` uniqueidentifier foreign key
- `actor_user_id` uniqueidentifier foreign key
- `recipient_user_id` uniqueidentifier foreign key
- `event_type` nvarchar(100)
- `idea_link` nvarchar(500)
- `message` nvarchar(500)
- `metadata_json` nvarchar(max)
- `occurred_at_utc` datetime2

### Suggested Constraints
- unique normalized email across the system on `normalized_email`
- unique normalized tag per organization on `(organization_id, normalized_name)`
- unique upvote per `(idea_id, user_id)`
- foreign keys from all tenant-owned entities back to `organization_id` where appropriate

### Audit Metadata Recommendations
- `created_at_utc`
- `updated_at_utc`
- `created_by_user_id`
- `updated_by_user_id`
- event-specific `message`
- structured `metadata`

## Domain Notes
- Site Admin is a global account and does not belong to an organization.
- Non-Site Admin users belong to exactly one organization.
- User accounts support `Active` and `Inactive` states only.
- Status deletion is soft-delete only.
- Comment bodies are plain text with line breaks only.

## Application Services and Workflows
### Authentication Services
- login service
- password policy service
- lockout tracking service
- current user service
- temporary password issuance service for P1

### Administration Services
- organization bootstrap service
- organization management service
- user management service
- role guard service

### Collaboration Services
- board management service
- status management service
- idea service
- tag normalization and resolution service
- mention parsing and resolution service
- comment service
- upvote service

### Cross-Cutting Services
- audit event writer
- notification event writer
- current user context provider
- UTC clock provider

## API Surface Outline
- Auth endpoints: login, organization resolution as needed, current user, and change password.
- Organization endpoints: create, edit, archive, list, and detail.
- User endpoints: create, edit, activate or inactivate, list, and detail.
- Board endpoints: create, edit, reorder swimlanes, list, and detail.
- Status endpoints: create, edit, soft-delete, list, and detail as needed.
- Idea endpoints: create, edit, list, detail, and status update.
- Engagement endpoints: comments, comment edits and deletes, upvotes, and mention resolution support.
- Notification handling: internal event generation in MVP, email delivery contracts later.

## Endpoint to Service Mapping
### Authentication
- `POST /api/v1/auth/login` -> `LoginService`, `LockoutTrackingService`, `AuditEventWriter`
- `GET /api/v1/auth/me` -> `CurrentUserService`
- `POST /api/v1/auth/change-password` -> `PasswordPolicyService`, `CurrentUserService`, `AuditEventWriter`
- `POST /api/v1/users/{userId}/temporary-password` -> `TemporaryPasswordIssuanceService`, `UserManagementService`, `AuditEventWriter`

### Organizations and Users
- `GET /api/v1/organizations` -> `OrganizationManagementService`
- `POST /api/v1/organizations` -> `OrganizationManagementService`, `OrganizationBootstrapService`, `AuditEventWriter`
- `GET /api/v1/organizations/{organizationId}` -> `OrganizationManagementService`
- `PUT /api/v1/organizations/{organizationId}` -> `OrganizationManagementService`, `AuditEventWriter`
- `POST /api/v1/organizations/{organizationId}/archive` -> `OrganizationManagementService`, `AuditEventWriter`
- `GET /api/v1/organizations/{organizationId}/users` -> `UserManagementService`
- `POST /api/v1/organizations/{organizationId}/users` -> `UserManagementService`, `PasswordPolicyService`, `AuditEventWriter`
- `GET /api/v1/users/{userId}` -> `UserManagementService`
- `PUT /api/v1/users/{userId}` -> `UserManagementService`, `RoleGuardService`, `AuditEventWriter`

### Boards and Statuses
- `GET /api/v1/organizations/{organizationId}/statuses` -> `StatusManagementService`
- `POST /api/v1/organizations/{organizationId}/statuses` -> `StatusManagementService`, `AuditEventWriter`
- `PUT /api/v1/statuses/{statusId}` -> `StatusManagementService`, `AuditEventWriter`
- `DELETE /api/v1/statuses/{statusId}` -> `StatusManagementService`, `AuditEventWriter`
- `GET /api/v1/organizations/{organizationId}/boards` -> `BoardManagementService`
- `POST /api/v1/organizations/{organizationId}/boards` -> `BoardManagementService`, `AuditEventWriter`
- `GET /api/v1/boards/{boardId}` -> `BoardManagementService`
- `PUT /api/v1/boards/{boardId}` -> `BoardManagementService`, `AuditEventWriter`
- `POST /api/v1/boards/{boardId}/swimlanes/reorder` -> `BoardManagementService`, `AuditEventWriter`

### Ideas and Engagement
- `GET /api/v1/boards/{boardId}/ideas` -> `IdeaService`
- `POST /api/v1/boards/{boardId}/ideas` -> `IdeaService`, `TagResolutionService`, `MentionResolutionService`, `AuditEventWriter`, `NotificationEventWriter`
- `GET /api/v1/ideas/{ideaId}` -> `IdeaService`
- `PUT /api/v1/ideas/{ideaId}` -> `IdeaService`, `TagResolutionService`, `MentionResolutionService`, `AuditEventWriter`, `NotificationEventWriter`
- `POST /api/v1/ideas/{ideaId}/status` -> `IdeaService`, `BoardManagementService`, `AuditEventWriter`, `NotificationEventWriter`
- `GET /api/v1/ideas/{ideaId}/comments` -> `CommentService`
- `POST /api/v1/ideas/{ideaId}/comments` -> `CommentService`, `MentionResolutionService`, `AuditEventWriter`, `NotificationEventWriter`
- `PUT /api/v1/comments/{commentId}` -> `CommentService`, `MentionResolutionService`, `AuditEventWriter`, `NotificationEventWriter`
- `DELETE /api/v1/comments/{commentId}` -> `CommentService`, `AuditEventWriter`
- `POST /api/v1/ideas/{ideaId}/upvote/toggle` -> `UpvoteService`, `AuditEventWriter`

## Client Composition Plan
### Primary Pages
- login page
- first-login password change page
- admin organizations page
- admin users page
- board overview page
- idea detail page or side panel
- status management page

### Shared Components
- application shell
- admin navigation rail
- command bar and filter components
- details list wrappers
- swimlane column component
- idea card component
- tag pill component
- mention picker
- comment thread component
- validation summary component

### State and Data Loading
- auth state provider for current user and role context
- page-level query and mutation services that wrap the API contracts
- explicit reload after critical mutations when authoritative state is needed

## Test Strategy
- Unit tests for authentication rules, first-login password change, organization-scoped authorization, lockout thresholds, board validation, tag normalization, mention resolution, comment validation, and upvote toggling.
- Integration tests for `/api/v1/auth/login`, protected endpoints, organization-scoped CRUD, board creation constraints, status lifecycle, and idea comment and upvote flows.
- Contract tests to keep `SPEC/30-Contracts.md` and OpenAPI aligned, including problem-details non-2xx responses.
- End-to-end tests for the seeded Site Admin flow, organization bootstrap, first-login password change, and core idea collaboration scenarios.

## Traceability Guidance
- map each implementation slice back to the relevant `SPEC/20-feature-*.md` file
- keep `SPEC/30-Contracts.md` and OpenAPI artifacts synchronized with endpoint changes
- use `SPEC/70-delivery-backlog.md` as the execution tracker derived from this plan

## Implementation Notes
- Put business rules in Domain and Application layers; keep controllers and components thin.
- Apply organization filtering centrally so the client cannot bypass it.
- Model notifications as events early, even if delivery is introduced in a later phase.
- Use admin-issued temporary password reset instead of self-service email reset in the current scope.
- Defer OAuth, reporting, guaranteed email delivery, event query endpoints, and remember-this-device until their specs are defined.
- Treat the original `SPEC` documents as the authoritative source and the Spec Kit port as an execution aid when there is any mismatch.
