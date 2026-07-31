# Tasks: Organizations and Users

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/20-feature-organizations-and-users.md`
	- `SPEC/40-test-strategy.md`
	- `SPEC/70-delivery-backlog.md`
- Last Canonical Sync Date: 2026-07-31

- [ ] O001 Implement organization create, detail, list, edit, and archive flows.
- [ ] O002 Bootstrap default statuses and one default board on organization creation.
- [ ] O003 Implement organization-scoped user create, detail, list, and edit flows.
- [ ] O004 Enforce org-scoped email uniqueness and active or inactive status.
- [ ] O005 Prevent the last Org Admin from removing their own access.
- [ ] O006 Emit audit events for organization and user changes.
- [ ] O007 Implement the canonical CSV template download and atomic user import across Application, API, Client, audit persistence, and unit/integration/contract tests, enforcing the 5 MB and 1,000-row limits, permitted roles, global email uniqueness, row-specific validation, and no plaintext password or file-content exposure.
