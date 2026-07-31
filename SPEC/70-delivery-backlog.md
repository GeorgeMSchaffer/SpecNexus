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
- Epic 9 (OAuth, post-MVP Phase 2) depends on MVP release completion and stable auth contracts.
- Epic 10 (SAML, post-OAuth) depends on Epic 9 completion.

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
- implement Development-only demo environment seed for 3 organizations with role-based users and forced first-login password change
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
- Infrastructure: organization, user, bootstrap, and atomic bulk-create persistence
- Application: organization bootstrap, archive flow, user CRUD, CSV import validation, lifecycle guardrails, last-Org-Admin protection
- API: organization and user endpoints, CSV template/import endpoints, filters, and validation
- Client: Admin section pages for organizations and users, including template download and CSV import feedback
- QA: org scope, lifecycle, archive behavior, template contract, and atomic import verification

Tasks:
- implement organization create, edit, detail, list, and archive flows
- auto-provision default statuses and one default board for each new organization
- implement user create, edit, detail, and list flows within organization scope
- provide the canonical downloadable user import CSV template
- implement atomic user CSV import for up to 1,000 rows and 5 MB with row-specific validation errors
- enforce one organization and one role per non-Site Admin user
- enforce globally unique email addresses
- support `Active` and `Inactive` user states
- prevent the last Org Admin from removing their own admin access or deactivating themselves
- emit audit events for organization and user administration actions

Exit criteria:
- Site Admin and Org Admin boundaries match the feature specs
- archived organizations remain retained and inaccessible according to policy
- authorized administrators can download the canonical template and import valid users without partial creation on validation failure

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
- add a critical-path smoke test for sign-in, board creation, and idea creation as a release-readiness gate
- verify seed behavior, default organization bootstrap, and audit generation end-to-end
- verify Development-only demo seed graph, idempotent startup behavior, and non-Development seed suppression
- confirm deferred work remains deferred

Deferred for MVP release:
- OAuth implementation (scheduled for post-MVP Phase 2)
- SAML implementation (scheduled after OAuth)

Exit criteria:
- the test strategy is covered by executable tests
- OpenAPI and written specs do not drift
- deferred items stay out of the MVP release

Dependencies:
- Epics 1 through 7

## Epic 9: OAuth/OIDC (Post-MVP Phase 2)
Outcome: organizations can authenticate through Microsoft Entra ID while preserving local login.

Suggested sequencing by team:
- Infrastructure: external identity persistence, provider configuration storage, and migration updates
- Application: external identity completion flow, linking/provisioning rules, and audit orchestration
- API: challenge/callback endpoints and OAuth configuration endpoints
- Client: organization-scoped OAuth entry-point UX and session completion handling
- QA: provider flow, coexistence, and provisioning matrix verification

Tasks:
- implement organization-scoped OAuth configuration management
- implement Microsoft Entra ID challenge and callback flow
- implement external identity linking and email fallback matching
- implement auto-provisioning for missing users with default role `User`
- preserve local login coexistence and break-glass Site Admin path
- emit audit events for OAuth outcomes and provisioning

Execution-ready lane slices:
- Infrastructure
	- O2-INF-01: add `ExternalIdentity` entity, mapping, and migration
	- O2-INF-02: add org-scoped OAuth provider configuration persistence
	- O2-INF-03: add secure provider option binding and validation plumbing
- Application
	- O2-APP-01: implement org-scoped OAuth start flow
	- O2-APP-02: implement OAuth callback completion and token-to-identity mapping
	- O2-APP-03: implement linking precedence and fallback matching
	- O2-APP-04: implement auto-provision with default role `User` and inactive-user denial
	- O2-APP-05: emit audit events for success/failure/provisioning outcomes
- API
	- O2-API-01: add OAuth start endpoint contract and routing
	- O2-API-02: add OAuth callback endpoint contract and routing
	- O2-API-03: add org-admin OAuth configuration CRUD endpoints
- Client
	- O2-CLI-01: add org-scoped OAuth sign-in action in login UX
	- O2-CLI-02: add callback completion and user-facing error states
	- O2-CLI-03: add org-admin OAuth configuration UX
- QA
	- O2-QA-01: add unit tests for linking precedence and provisioning decisions
	- O2-QA-02: add integration tests for callback success, inactive-user denial, and provisioning
	- O2-QA-03: add regression tests for local login coexistence and break-glass access

Exit criteria:
- Microsoft Entra ID sign-in works for configured organizations
- local login remains functional and unchanged for non-SSO flows
- linking and provisioning behavior matches feature contracts

Dependencies:
- MVP release complete
- Epic 2 authentication contracts stable

## Epic 10: SAML (Post-OAuth Phase)
Outcome: organizations can authenticate with SAML 2.0 using the same identity-linking model.

Suggested sequencing by team:
- Infrastructure: SAML provider configuration storage and certificate/metadata handling
- Application: assertion-to-identity mapping and provisioning flow reuse
- API: SAML initiation/callback surfaces and configuration endpoints
- Client: SAML entry-point UX and coexistence behavior
- QA: protocol validation, assertion behavior, and regression verification

Tasks:
- implement organization-scoped SAML configuration
- implement SP-initiated SAML flow
- reuse OAuth-established identity linking and auto-provisioning model
- implement protocol validation and audit coverage

Execution-ready lane slices:
- Infrastructure
	- S3-INF-01: add SAML metadata and certificate-reference persistence
	- S3-INF-02: add metadata refresh and certificate rotation support
- Application
	- S3-APP-01: implement org-scoped SAML initiation orchestration
	- S3-APP-02: implement assertion-consumer mapping into shared external identity model
	- S3-APP-03: reuse OAuth linking and provisioning policy logic
	- S3-APP-04: emit audit events for SAML success/failure/provisioning outcomes
- API
	- S3-API-01: add SAML start endpoint contract and routing
	- S3-API-02: add SAML assertion-consumer endpoint contract and routing
	- S3-API-03: add org-admin SAML configuration endpoints and metadata validation responses
- Client
	- S3-CLI-01: add SAML sign-in action in organization login UX
	- S3-CLI-02: add admin SAML configuration UX and validation messaging
	- S3-CLI-03: preserve local + OAuth coexistence affordances during SAML rollout
- QA
	- S3-QA-01: add protocol validation tests for malformed/expired/mismatched assertions
	- S3-QA-02: add integration tests for mapping, linking, and provisioning outcomes
	- S3-QA-03: add coexistence regression matrix for local, OAuth, and SAML paths

Exit criteria:
- SAML sign-in works for configured organizations
- local and OAuth login paths continue to function
- SAML behavior matches acceptance criteria

Dependencies:
- Epic 9

## Recommended Execution Order
1. Complete Epic 1 fully before parallelizing downstream work.
2. Run Epic 2 next because authentication gates all protected workflows.
3. Run Epic 3 and the schema portion of Epic 4 in close sequence because tenant bootstrap creates default boards and statuses.
4. Finish Epic 4 before starting broad Idea and Engagement implementation in Epic 5.
5. Add Epic 6 once core collaboration events exist so event shapes settle late but before hardening.
6. Run Epic 7 continuously after API slices stabilize, but reserve final UI completion until Epics 2 through 5 are functionally complete.
7. Close with Epic 8 as the release gate.
8. Start Epic 9 only after MVP release criteria are met.
9. Execute Epic 10 after Epic 9 stabilizes.
