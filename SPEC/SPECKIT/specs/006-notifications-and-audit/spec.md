# Feature Specification: Notifications and Audit

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
