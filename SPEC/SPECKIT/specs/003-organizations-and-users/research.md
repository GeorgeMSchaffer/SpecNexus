# Research: Organizations and Users

## Key Decisions
- organizations are archived, not hard-deleted
- new organizations receive default statuses and one default board
- user emails are unique within an organization only
- user lifecycle states are `Active` and `Inactive`

## Risk Areas
- bootstrapping organizations atomically with default data
- preventing last-Org-Admin lockout scenarios
- preserving auditability of user role and status changes
