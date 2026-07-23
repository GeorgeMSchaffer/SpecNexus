# Data Model: Notifications and Audit

## NotificationEvent
- `notificationEventId`
- `organizationId`
- `boardId`
- `ideaId`
- `actorUserId`
- `recipientUserId`
- `eventType`
- `ideaLink`
- `occurredAtUtc`
- `metadata`

## AuditEvent
- `auditEventId`
- `organizationId` nullable
- `actorUserId`
- `eventType`
- `entityType`
- `entityId`
- `occurredAtUtc`
- `metadata`
