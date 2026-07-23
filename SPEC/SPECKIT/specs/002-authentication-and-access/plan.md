# Plan: Authentication and Access

## Scope
- login flow
- password verification and policy enforcement
- lockout behavior
- first-login password change
- auth audit events

## Technical Notes
- keep auth endpoints under `/api/v1/auth`
- use problem-details for error states
- enforce tenant-aware behavior in the Application layer
