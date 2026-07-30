# Feature Specification: Organizations and Users

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/10-requirements.md`
	- `SPEC/20-feature-organizations-and-users.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-07-30

## Summary
Implement organization lifecycle, tenant bootstrap, user administration, role boundaries, user lifecycle state management, and organization branding controls.

## Requirements
- Site Admin creates organizations.
- Organizations can be edited and archived, but not hard-deleted.
- New organizations receive default statuses and one default board.
- In Development only, startup seed creates 3 demo organizations.
- Each seeded demo organization includes one Org Admin, one User, and one Read Only account.
- Admin views use guided empty states with primary actions and short explanatory text.
- Admin views use a unified application shell with persistent header, role-aware navigation, and breadcrumb navigation.
- Non-Site Admin users belong to exactly one organization and one role.
- Email is globally unique across the system.
- User lifecycle states are `Active` and `Inactive` only.
- The last Org Admin cannot remove their own admin access or deactivate themselves.
- Organization and user administration actions are audited.
- Organization edit supports logo upload with in-form thumbnail preview.
- Exactly one logo is active per organization; new logo upload replaces prior logo.
- Organization logos are rendered with max height `150px` while preserving aspect ratio.
