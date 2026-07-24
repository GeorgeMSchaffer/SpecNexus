# Feature Specification: Organizations and Users

## Summary
Implement organization lifecycle, tenant bootstrap, user administration, role boundaries, and user lifecycle state management.

## Requirements
- Site Admin creates organizations.
- Organizations can be edited and archived, but not hard-deleted.
- New organizations receive default statuses and one default board.
- Admin views use guided empty states with primary actions and short explanatory text.
- Non-Site Admin users belong to exactly one organization and one role.
- Email is globally unique across the system.
- User lifecycle states are `Active` and `Inactive` only.
- The last Org Admin cannot remove their own admin access or deactivate themselves.
- Organization and user administration actions are audited.
