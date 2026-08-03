# Test Strategy

## Unit
- AuthService credential validation
- Seed Site Admin first-login password change rule
- Development demo seed creates expected organizations, users by role, boards, swimlanes, ideas, and comments
- Development demo seed is idempotent across repeated startup execution
- Organization-scoped authorization checks
- User CSV parsing, trimming, default status, allowed role, row-count, and file-size validation
- User CSV import rejects duplicate emails within the file and performs no persistence when any row is invalid
- Lockout threshold of 5 failed attempts in 15 minutes and 15-minute expiration rules
- Board validation rules
- Tag normalization and create-on-save behavior
- Mention resolution within an organization
- Upvote toggle behavior

## Integration
- `/api/v1/auth/login`: success, invalid credential, and 15-minute lockout branches
- protected endpoints reject unauthenticated requests
- seeded Site Admin is forced through password change on first login
- Development startup auto-seeds exactly 3 demo organizations
- each demo organization includes Org Admin, User, and Read Only accounts initialized to `abc123!` and forced password change
- each demo organization has one seeded example board with ideas across each default swimlane and example comments
- organization CRUD follows Site Admin and Org Admin role boundaries
- user CRUD is limited to the correct organization scope
- user CSV template downloads with the canonical content type, filename, header order, and example row
- valid user CSV import creates every row in the selected organization and returns the created count
- user CSV import enforces Site Admin and Org Admin organization scope and rejects Site Admin as an imported role
- invalid, duplicate, empty, oversized, and over-row-limit CSV imports return row-specific validation where applicable and create no users
- user CSV import responses, logs, and audit events do not expose plaintext initial passwords or uploaded file contents
- board creation rejects fewer than 2 swimlanes
- idea comment and upvote flows enforce role rules

## Contract
- Response schema validation against the published OpenAPI documents
- Problem-details-style error envelope required for all non-2xx responses
- Authentication, organization, user, board, status, and idea contracts stay aligned with `30-Contracts.md`
- User CSV template and import content types, request limits, response shape, and problem-details errors stay aligned with `30-Contracts.md`

## Smoke Tests
- Critical-path smoke test: sign in successfully, create a new board, and create a new idea
- Smoke test success criteria:
  - the user can authenticate with a seeded account and reach the main workspace experience
  - a board can be created with the expected default status structure and saved successfully
  - an idea can be created on the new board and appears in the board view without validation errors
- Smoke test is intended as a release-readiness check for the MVP critical workflow, alongside the detailed unit, integration, and contract coverage

## Startup Safety
- Demo environment seed runs only in Development
- Non-Development startup does not apply demo data seed

## MVP Scope Gate
- MVP release verification does not require OAuth or SAML endpoint implementation.
- OAuth validation is executed in post-MVP Phase 2 test cycles.
- SAML validation is executed in a post-OAuth phase test cycle.

## Client UI — /ideas Kanban Board
- Card rendering: verify each card shows title, priority badge, assignee name, and upvote count; verify created-date and tag chips are absent from the compact card face.
- Title-click overlay: verify clicking the card title opens the in-context detail overlay without page navigation; verify the overlay displays all fields (title, priority, due date, description, assignee, tags, mentions, comments) and provides Cancel, Save Idea, and Move in Board actions.
- New Idea button: verify the New Idea button appears in the board header for all roles except ReadOnly; verify it opens the overlay in create mode with the left-most status pre-selected.
- Card drag-and-drop: verify optimistic column move is applied immediately, `MoveIdeaStatusAsync` is called with the correct target status ID, the idea's status is set to the target swimlane's status, and the card reverts to its original column with an error toast on API failure.
- Column reorder: verify SiteAdmin and OrgAdmin users can reorder columns and `UpdateStatusAsync` is called per affected status; verify reorder saves immediately on drop without a confirmation step; verify User and ReadOnly users cannot trigger a reorder; verify all columns revert on API failure with error toast.
- Filter chips and search: verify All / Created by me / Assigned to me filter chips correctly hide non-matching cards client-side; verify search matches by title, tag, and assignee name (case-insensitive); verify search is combinable with filter chips; verify empty columns display the "No ideas" placeholder.