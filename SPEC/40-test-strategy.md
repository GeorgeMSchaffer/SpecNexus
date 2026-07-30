# Test Strategy

## Unit
- AuthService credential validation
- Seed Site Admin first-login password change rule
- Development demo seed creates expected organizations, users by role, boards, swimlanes, ideas, and comments
- Development demo seed is idempotent across repeated startup execution
- Organization-scoped authorization checks
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
- board creation rejects fewer than 2 swimlanes
- idea comment and upvote flows enforce role rules

## Contract
- Response schema validation against the published OpenAPI documents
- Problem-details-style error envelope required for all non-2xx responses
- Authentication, organization, user, board, status, and idea contracts stay aligned with `30-Contracts.md`

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