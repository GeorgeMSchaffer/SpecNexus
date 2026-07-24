# Data Model: SargentNexus MVP

## Core Entities

### Organization
- `organizationId`
- `companyName`
- `address`
- `city`
- `state`
- `zip`
- `phone`
- `primaryContactFirstName`
- `primaryContactLastName`
- `isArchived`

Rules:
- tenant ownership boundary for business data
- archived, not hard-deleted
- bootstrapped with default statuses and one default board

### User
- `userId`
- `organizationId` nullable for Site Admin only
- `firstName`
- `lastName`
- `email`
- `passwordHash` automatically generated when the user is created or changes password
- `role`
- `status`
- `mustChangePassword`

Rules:
- non-Site Admin users belong to exactly one organization
- email is globally unique across the system
- status limited to `Active` and `Inactive`

### Status
- `statusId`
- `organizationId`
- `name`
- `isDeleted`

Rules:
- soft-delete only
- default set provisioned on new organization creation

### Board
- `boardId`
- `organizationId`
- `name`

### BoardSwimlane
- `boardId`
- `statusId`
- `order`

Rules:
- minimum of two swimlanes per board
- order persisted immediately after reorder

### Idea
- `ideaId`
- `boardId`
- `organizationId`
- `authorUserId`
- `title`
- `description`
- `statusId`
- `createdAtUtc`
- `updatedAtUtc`

Rules:
- title max 150
- description max 4000
- completed ideas remain editable and collaborative

### Tag
- `tagId`
- `organizationId`
- `name`
- `normalizedName`

Rules:
- max 100 characters
- trimmed and case-insensitive unique within an organization

### IdeaTag
- `ideaId`
- `tagId`

### Mention
- `mentionId`
- `organizationId`
- `ideaId` nullable when mention belongs to a comment-only context record
- `commentId` nullable
- `mentionedUserId`
- `sourceText`

Rules:
- resolved by organization-scoped email lookup

### Comment
- `commentId`
- `ideaId`
- `authorUserId`
- `body`
- `createdAtUtc`
- `updatedAtUtc`

Rules:
- chronological display
- author can edit and delete own comment
- admins can delete in scope

### Upvote
- `ideaId`
- `userId`
- `createdAtUtc`

Rules:
- one active upvote per user per idea
- only casting user can remove it

### AuditEvent
- `auditEventId`
- `organizationId` nullable
- `actorUserId`
- `eventType`
- `entityType`
- `entityId`
- `occurredAtUtc`
- `metadata`

### NotificationEvent
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
