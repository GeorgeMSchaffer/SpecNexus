# Research Notes: SargentNexus MVP

## Confirmed Constraints
- Do not add new packages without approval.
- Follow the established .NET layered architecture.
- Use SQL Server 2022, EF Core, ASP.NET Core Web API, Blazor, and Fluent UI.
- Keep OAuth, MFA, reporting, and guaranteed email delivery out of MVP.

## Important Product Decisions
- Site Admin is global and does not belong to a tenant.
- Duplicate emails across organizations are allowed, so login may require organization selection.
- Password reset is admin-issued temporary password only in P1.
- Notifications are modeled as events now, not guaranteed outbound delivery in MVP.
- Update operations use last-write-wins semantics in MVP.

## Risk Areas
- Duplicate-email login requires careful auth contract design and UI handling.
- Global Site Admin must not break assumptions that all users are tenant-owned.
- Board reorder persistence and idea status changes need clear authorization and audit generation.
- Tag normalization and concurrent duplicate creation need deterministic database and application behavior.

## Verification Priorities
- login and lockout behavior
- organization bootstrap behavior
- role boundaries and authorization scope
- tag normalization and mention resolution
- audit and notification event generation
