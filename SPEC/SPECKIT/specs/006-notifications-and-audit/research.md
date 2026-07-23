# Research: Notifications and Audit

## Key Decisions
- notification events are generated in MVP
- outbound email delivery is deferred outside MVP
- one event maps to one email when delivery exists later
- audit covers auth, admin, and idea lifecycle actions

## Risk Areas
- retaining enough context for later email delivery without implementing the mail pipeline yet
- ensuring audit and notification events are observable in tests
