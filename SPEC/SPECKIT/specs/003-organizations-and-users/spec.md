# Feature Specification: Organizations and Users

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/10-requirements.md`
	- `SPEC/20-feature-organizations-and-users.md`
	- `SPEC/20-feature-client-ui-revisions.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-07-30

## Summary
Implement organization lifecycle, tenant bootstrap, user administration, role boundaries, user lifecycle state management, invite-code-based self-registration, and organization branding controls.

## Requirements
- Site Admin creates organizations.
- Organizations can be edited and archived, but not hard-deleted.
- New organizations receive default statuses and one default board.
- An invite code is auto-generated when an organization is created.
- Invite codes are displayed in the organization list and detail views.
- Admins can regenerate an organization's invite code.
- Invite codes for archived organizations are invalid for self-registration.
- Users can self-register using a valid invite code; the code determines which organization they are associated with.
- In Development only, startup seed creates 3 demo organizations.
- Each seeded demo organization includes one Org Admin, one User, and one Read Only account.
- Settings screens (formerly Admin) use guided empty states with primary actions and short explanatory text.
- Settings use a unified application shell with persistent header, role-aware navigation, and breadcrumb navigation.
- The header uses `rgb(33, 37, 41)` background; username text is white; sign-out icon is left of the username; a gear icon navigates to Settings.
- Non-Site Admin users belong to exactly one organization and one role.
- Email is globally unique across the system.
- User lifecycle states are `Active` and `Inactive` only.
- The last Org Admin cannot remove their own admin access or deactivate themselves.
- Organization and user administration actions are audited.
- Organization edit supports logo upload with in-form thumbnail preview.
- Exactly one logo is active per organization; new logo upload replaces prior logo.
- Organization logos are rendered with max height `150px` while preserving aspect ratio.
