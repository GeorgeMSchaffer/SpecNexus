# Feature: Notifications

## Outcome
Users receive email notifications for collaboration events relevant to them.

## Scope
- In for later phase: queued email delivery for collaboration events
- Out for MVP: guaranteed email delivery implementation

## Notification Triggers
Email notifications are sent when:
1. A user is @mentioned in an idea
2. A user is @mentioned in a comment
3. A comment is added to a user's idea
4. An idea's status changes

## Recipients
- @mention notification → mentioned user
- comment @mention notification → mentioned user
- Comment notification → idea author
- Status change notification → idea author

## Delivery Rules
1. Notification emails must include the idea title.
2. Notification emails must include a canonical idea link using the organization-aware board route pattern `/org/{organizationId}/boards/{boardId}/ideas/{ideaId}`.
3. Mention notifications should include relevant idea context.
4. Email delivery is optional for MVP and is implemented in a later phase.
5. The initial notification feature does not include per-user notification preference settings.
6. When notifications are enabled, each event generates its own email rather than consolidating multiple events into one message.
7. Notification events are persisted for internal processing and verification only in MVP; read or query endpoints are not required in MVP.
8. MVP verification uses tests and internal diagnostics outside the public API surface rather than public event query endpoints.

## Acceptance Criteria
- [ ] Notification triggers are defined for mentions, comments, and status changes
- [ ] Email delivery remains deferred outside MVP
- [ ] The initial notification feature does not include per-user opt-out preferences
- [ ] Later-phase notifications send one email per event
- [ ] Later-phase notification emails include the canonical organization-aware board route to the idea
- [ ] Notification events do not require read or query endpoints in MVP
- [ ] MVP event verification is handled through tests and internal diagnostics outside the public API surface