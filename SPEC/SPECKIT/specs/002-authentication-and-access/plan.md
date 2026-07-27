# Plan: Authentication and Access

## Scope
- login flow
- password verification and policy enforcement
- lockout behavior
- first-login password change
- auth audit events

## Exclusions
- OAuth/OIDC implementation is deferred to post-MVP Phase 2.
- SAML implementation is deferred to a post-OAuth phase.

## Technical Notes
- keep auth endpoints under `/api/v1/auth`
- use problem-details for error states
- enforce tenant-aware behavior in the Application layer

## Post-MVP Execution References
- OAuth/OIDC (Phase 2) execution slices are tracked in `SPEC/70-delivery-backlog.md` under Epic 9 task IDs `O2-*`.
- SAML (post-OAuth phase) execution slices are tracked in `SPEC/70-delivery-backlog.md` under Epic 10 task IDs `S3-*`.
