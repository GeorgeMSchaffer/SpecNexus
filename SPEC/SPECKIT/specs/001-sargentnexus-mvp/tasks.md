# Tasks: SargentNexus MVP

## Phase 1: Foundation
- [ ] T001 Create the solution structure and project references.
- [ ] T002 Add the EF Core DbContext, migrations, and audit storage foundations.
- [ ] T003 Implement shared `/api/v1` routing conventions and problem-details error handling.
- [ ] T004 Implement API validation plumbing and OpenAPI scaffolding.

## Phase 2: Authentication and Global Administration
- [ ] T005 Implement login request handling with optional organization resolution.
- [ ] T006 [P] Implement password hashing and verification.
- [ ] T007 [P] Enforce password complexity, inactive-account denial, and 5-in-15 lockout rules.
- [ ] T008 Seed the global Site Admin from an environment-provided initial credential.
- [ ] T009 Implement forced first-login password change for the seeded Site Admin.
- [ ] T010 Emit audit events for authentication outcomes and password changes.
- [ ] T011 Implement admin-issued temporary password reset as the P1 extension path.

## Phase 3: Organizations and Users
- [ ] T012 Implement organization create, detail, list, edit, and archive flows.
- [ ] T013 Automatically provision default statuses and one default board during organization creation.
- [ ] T014 Implement organization-scoped user create, detail, list, and edit flows.
- [ ] T015 Enforce one organization and one role for each non-Site Admin user.
- [ ] T016 Enforce email uniqueness within an organization.
- [ ] T017 Support `Active` and `Inactive` user states.
- [ ] T018 Prevent the last Org Admin from removing their own admin access or deactivating themselves.
- [ ] T019 Emit audit events for organization and user administration actions.

## Phase 4: Boards and Statuses
- [ ] T020 Implement organization-scoped status create, update, list, and soft-delete flows.
- [ ] T021 Implement board create, update, list, and detail flows.
- [ ] T022 Enforce minimum two swimlanes per board.
- [ ] T023 Support board subsets of organization statuses.
- [ ] T024 Persist swimlane reorder immediately after drag-and-drop.

## Phase 5: Ideas and Engagement
- [ ] T025 Implement idea create, detail, list, update, and status change flows.
- [ ] T026 Enforce title and description limits.
- [ ] T027 Implement default left-most-swimlane status assignment on create.
- [ ] T028 Implement tag autocomplete after 2 characters.
- [ ] T029 Implement tag normalization, uniqueness, and merge-on-concurrency behavior.
- [ ] T030 Implement organization-scoped email-based mention resolution for ideas and comments.
- [ ] T031 Implement comment create, edit, delete, and chronological retrieval flows.
- [ ] T032 Implement upvote toggle with one active upvote per user per idea.
- [ ] T033 Restrict upvote removal to the user who cast it.
- [ ] T034 Allow board-configured Users to move any idea on an eligible board.
- [ ] T035 Keep completed ideas editable and collaborative.
- [ ] T036 Emit audit events for idea creation, edits, status changes, comments, and upvotes.

## Phase 6: Notification Events
- [ ] T037 Emit notification events for idea mentions, comment mentions, idea comments, and status changes.
- [ ] T038 Persist canonical idea links on notification events.
- [ ] T039 Keep outbound email delivery explicitly deferred outside MVP.

## Phase 7: Client Experience
- [ ] T040 Build the login flow including organization selection when required.
- [ ] T041 Build first-login password change and inactive-account UI states.
- [ ] T042 Build the Admin section for organizations and users.
- [ ] T043 Build board and status administration workflows.
- [ ] T044 Build idea detail, tags, mentions, comments, and upvote workflows.
- [ ] T045 Reflect Site Admin, Org Admin, User, and Read Only boundaries in the UI.

## Phase 8: Hardening and Validation
- [ ] T046 Align OpenAPI with the written contracts.
- [ ] T047 Implement unit tests from the test strategy.
- [ ] T048 Implement integration tests for auth, organization scope, and collaboration flows.
- [ ] T049 Implement contract tests for schema and problem-details error behavior.
- [ ] T050 Verify seed behavior, organization bootstrap, audit generation, and deferred-scope boundaries end-to-end.
