# Feature Specification: Notifications and Audit

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/20-feature-notifications.md`
	- `SPEC/20-feature-auth.md`
	- `SPEC/20-feature-organizations-and-users.md`
	- `SPEC/20-feature-ideas-and-engagement.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-07-30

## Summary
Generate notification events for collaboration activity and audit events for security-sensitive and workflow-changing actions, while deferring guaranteed outbound email delivery.

## Requirements
- Notification events are generated for idea mentions, comment mentions, comments on ideas, and idea status changes.
- Notification events carry canonical idea links.
- Guaranteed email delivery remains outside MVP.
- Event verification in MVP uses tests and internal diagnostics outside the public API surface.
- The initial notification feature has no per-user opt-out preferences.
- When notification delivery is introduced, one event maps to one email.
- Audit events cover auth actions, admin actions, and idea lifecycle actions.
