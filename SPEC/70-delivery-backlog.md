# Delivery Backlog

## Purpose
Break the technical implementation plan into execution-ready epics and task slices.

## Team Lanes
- API: HTTP contracts, endpoint mapping, OpenAPI, auth middleware, problem-details responses
- Application: use cases, authorization, orchestration, business rules, audit and notification event triggering
- Infrastructure: EF Core mappings, migrations, database constraints, password hashing, token issuance, environment-seeded credentials
- Client: Blazor pages, Fluent UI layouts, workflow composition, role-aware affordances
- QA: contract, integration, end-to-end, and regression validation

## Dependency Rules
- Epics 2 through 7 depend on Epic 1 foundation work.
- Epic 3 depends on authentication primitives from Epic 2 for user and role handling.
- Epic 4 depends on organization bootstrap from Epic 3.
- Epic 5 depends on Epic 4 because ideas require board and status configuration.
- Epic 6 depends on Epic 5 because notification and audit surfaces attach to collaboration workflows.
- Epic 7 can begin after Epics 2 through 5 expose stable API contracts for each feature area.
- Epic 8 depends on executable slices from all previous epics.

## Epic 1: Foundation and Contract Baseline
Outcome: the solution boots, persists data, and exposes a consistent API shell.

Suggested sequencing by team:
- Infrastructure: base solution, DbContext, migrations, shared persistence primitives
- API: routing conventions, problem-details envelope, validation pipeline, OpenAPI skeleton
- Application: current user context, clock abstraction, shared cross-cutting interfaces
- QA: baseline smoke checks for boot and error envelope behavior

Tasks:
- create the solution structure and project references
- add the EF Core DbContext, migrations, and base audit storage strategy
- implement shared `/api/v1` routing conventions
- implement problem-details error handling
- implement API request validation and OpenAPI scaffolding

Exit criteria:
- the API starts successfully
- migrations can create the base schema
- non-2xx responses use the standard error envelope

Dependencies:
- none

## Epic 2: Authentication and Global Administration
Outcome: users can authenticate safely and the global Site Admin bootstrap path works.

Suggested sequencing by team:
- Infrastructure: password hashing, token generation, seed credential loading, lockout persistence
- Application: login, lockout rules, current-user service, password-change workflow
- API: auth endpoints and authorization integration
- Client: login and first-login password change screen
- QA: auth matrix and lockout verification

Tasks:
- implement login with globally unique email credentials
- implement password hashing and verification
- enforce password complexity rules
- enforce inactive-account denial
- implement 5-failures-in-15-minutes lockout for 15 minutes
- seed the global Site Admin with an environment-provided initial credential
- force first-login password change for the seeded Site Admin
- emit audit events for login outcomes and password actions

Follow-up:
- add admin-issued temporary password reset

Exit criteria:
- login success, invalid credentials, inactive account, and lockout flows work end-to-end
- the seeded Site Admin can sign in and is forced through password change

Dependencies:
- Epic 1

## Epic 3: Organizations and Users
Outcome: tenant administration works with the required role boundaries.

Suggested sequencing by team:
- Infrastructure: organization, user, and bootstrap persistence
- Application: organization bootstrap, archive flow, user CRUD, lifecycle guardrails, last-Org-Admin protection
- API: organization and user endpoints, filters, and validation
- Client: Admin section pages for organizations and users
- QA: org scope, lifecycle, and archive behavior verification

Tasks:
- implement organization create, edit, detail, list, and archive flows
- auto-provision default statuses and one default board for each new organization
- implement user create, edit, detail, and list flows within organization scope
- enforce one organization and one role per non-Site Admin user
- enforce globally unique email addresses
- support `Active` and `Inactive` user states
- prevent the last Org Admin from removing their own admin access or deactivating themselves
- emit audit events for organization and user administration actions

Exit criteria:
- Site Admin and Org Admin boundaries match the feature specs
- archived organizations remain retained and inaccessible according to policy

Dependencies:
- Epics 1 and 2

## Epic 4: Boards and Statuses
Outcome: organizations can manage workflow lanes safely.

Suggested sequencing by team:
- Infrastructure: statuses, boards, and swimlane mapping schema
- Application: status soft-delete rules, board validation, reorder behavior
- API: board and status endpoints
- Client: board admin and status admin surfaces
- QA: board constraints and historical status labeling checks

Tasks:
- implement organization-scoped status create, edit, list, and soft-delete flows
- seed the default status set on organization creation
- implement board create, edit, list, and detail flows
- enforce minimum two swimlanes per board
- support board subsets of organization statuses
- implement immediate swimlane reorder persistence

Exit criteria:
- new organizations have a usable default board and status set
- swimlane reorder is persisted immediately

Dependencies:
- Epics 1 and 3

## Epic 5: Ideas and Engagement
Outcome: users can create and collaborate on ideas inside their organization.

Suggested sequencing by team:
- Infrastructure: ideas, comments, tags, mentions, and upvotes schema plus uniqueness constraints
- Application: idea workflows, tag normalization, mention resolution, comment permissions, upvote toggling
- API: idea, comment, upvote, and status-move endpoints
- Client: board cards, idea detail, comments, tags, mention picker, upvote actions
- QA: collaboration and role matrix verification

Tasks:
- implement idea create, edit, detail, list, and status update flows
- enforce title and description constraints
- default new idea status to the left-most board swimlane when not supplied
- implement tag autocomplete after 2 characters
- implement tag normalization and merge-on-concurrency behavior
- implement email-based mention resolution for ideas and comments
- implement comment create, edit, delete, and chronological retrieval
- implement upvote toggle with one active upvote per user per idea
- allow board-configured user status changes for any idea on the board
- keep completed ideas editable and collaborative
- emit audit events for idea lifecycle actions

Exit criteria:
- Read Only, User, Org Admin, and Site Admin permissions match the feature specs
- collaboration behavior matches the acceptance criteria

Dependencies:
- Epics 1, 3, and 4

## Epic 6: Notification Events and Audit Surfaces
Outcome: the application records collaboration events needed for later notification delivery.

Suggested sequencing by team:
- Infrastructure: audit and notification event persistence
- Application: event composition and message generation
- API: no new read endpoints required in MVP unless scope changes
- Client: optional internal diagnostics only if explicitly needed
- QA: event persistence verification through tests or internal inspection tools

Tasks:
- emit notification events for idea mentions, comment mentions, comments on ideas, and status changes
- persist canonical idea links with notification event context
- expose or store audit and notification events in an observable way for verification
- keep email delivery explicitly deferred outside MVP

Exit criteria:
- notification events are generated correctly without requiring outbound email delivery
- audit coverage exists for auth, admin, and idea lifecycle actions

Dependencies:
- Epics 1 through 5

## Epic 7: Blazor Client Experience
Outcome: the client supports the agreed workflows and role boundaries.

Suggested sequencing by team:
- Client: page shells, shared components, admin flows, board flows, detail views
- API/Application: close support loop for UI-driven gaps discovered during composition
- QA: navigation, validation, and role-affordance checks

Tasks:
- build the login flow for globally unique email credentials
- build first-login password change and inactive-account states
- build the Admin section for organizations and users
- build board and status administration flows
- build board, idea detail, comment, tag, mention, and upvote interfaces
- enforce visible role-based UI affordances for Read Only, User, Org Admin, and Site Admin

Exit criteria:
- the client exposes each MVP workflow without relying on undefined behavior

Dependencies:
- Epics 2 through 5 primarily, with Epic 6 optional for internal diagnostics only

## Epic 8: Hardening and Release Readiness
Outcome: contracts, tests, and release boundaries are aligned.

Suggested sequencing by team:
- QA: contract, integration, and end-to-end coverage
- API: OpenAPI and contract drift correction
- Application/Infrastructure: fix workflow, data, or event discrepancies surfaced by tests
- Client: final UX defect correction and polish within approved scope

Tasks:
- align OpenAPI with the contracts document endpoint by endpoint
- implement unit tests from the test strategy
- implement integration tests for auth, protected routes, organization scope, and collaboration flows
- implement contract tests for schemas and problem-details error responses
- verify seed behavior, default organization bootstrap, and audit generation end-to-end
- confirm deferred work remains deferred

Exit criteria:
- the test strategy is covered by executable tests
- OpenAPI and written specs do not drift
- deferred items stay out of the MVP release

Dependencies:
- Epics 1 through 7

## Recommended Execution Order
1. Complete Epic 1 fully before parallelizing downstream work.
2. Run Epic 2 next because authentication gates all protected workflows.
3. Run Epic 3 and the schema portion of Epic 4 in close sequence because tenant bootstrap creates default boards and statuses.
4. Finish Epic 4 before starting broad Idea and Engagement implementation in Epic 5.
5. Add Epic 6 once core collaboration events exist so event shapes settle late but before hardening.
6. Run Epic 7 continuously after API slices stabilize, but reserve final UI completion until Epics 2 through 5 are functionally complete.
7. Close with Epic 8 as the release gate.
