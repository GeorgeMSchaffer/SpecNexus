# API Contracts: SargentNexus MVP

## Shared Conventions
- Base path: `/api/v1`
- Identifiers: GUID strings
- Errors: problem-details style payloads
- Pagination: `items`, `page`, `pageSize`, `totalCount`, `sortBy`, `sortDirection`

## Authentication

### `POST /api/v1/auth/login`
Request:
- `email` required string
- `password` required string
- `organizationId` optional GUID

Responses:
- `200` authenticated response with `accessToken`, `expiresInSeconds`, `requiresPasswordChange`, and `user`
- `200` organization selection response with `requiresOrganizationSelection` and `organizations`
- `401` invalid credentials
- `403` inactive account
- `429` lockout state

### `GET /api/v1/auth/me`
Response:
- current authenticated user summary

### `POST /api/v1/auth/change-password`
Request:
- `currentPassword`
- `newPassword`

Response:
- `204 No Content`

## Organizations

### `GET /api/v1/organizations`
Paginated list for Site Admin with optional search, archive filter, and one sort field.

### `POST /api/v1/organizations`
Creates an organization and returns `organizationId`, `defaultBoardId`, and `defaultStatusCount`.

### `GET /api/v1/organizations/{organizationId}`
Returns organization detail.

### `PUT /api/v1/organizations/{organizationId}`
Updates organization detail.

### `POST /api/v1/organizations/{organizationId}/archive`
Archives an organization.

## Users

### `GET /api/v1/organizations/{organizationId}/users`
Paginated organization-scoped user list with search, role, status, and one sort field.

### `POST /api/v1/organizations/{organizationId}/users`
Creates a user in the target organization.

### `GET /api/v1/users/{userId}`
Returns user detail.

### `PUT /api/v1/users/{userId}`
Updates user profile, role, or status.

### `POST /api/v1/users/{userId}/temporary-password`
P1 admin-issued temporary password reset.

## Statuses

### `GET /api/v1/organizations/{organizationId}/statuses`
Returns organization statuses.

### `POST /api/v1/organizations/{organizationId}/statuses`
Creates a status.

### `PUT /api/v1/statuses/{statusId}`
Updates a status.

### `DELETE /api/v1/statuses/{statusId}`
Soft-deletes a status.

## Boards

### `GET /api/v1/organizations/{organizationId}/boards`
Returns organization boards.

### `POST /api/v1/organizations/{organizationId}/boards`
Creates a board with at least two swimlanes.

### `GET /api/v1/boards/{boardId}`
Returns board detail and swimlane ordering.

### `PUT /api/v1/boards/{boardId}`
Updates board name and swimlane set.

### `POST /api/v1/boards/{boardId}/swimlanes/reorder`
Persists immediate swimlane reorder.

## Ideas

### `GET /api/v1/boards/{boardId}/ideas`
Paginated idea list with basic filtering and one sort field.

### `POST /api/v1/boards/{boardId}/ideas`
Creates an idea using title, description, optional status, optional tag names, and optional mention emails.

### `GET /api/v1/ideas/{ideaId}`
Returns full idea detail including tags, mentions, comments, and upvote count.

### `PUT /api/v1/ideas/{ideaId}`
Updates idea title, description, tags, and mentions.

### `POST /api/v1/ideas/{ideaId}/status`
Moves an idea to a new status.

## Comments

### `GET /api/v1/ideas/{ideaId}/comments`
Paginated chronological comment list.

### `POST /api/v1/ideas/{ideaId}/comments`
Creates a comment.

### `PUT /api/v1/comments/{commentId}`
Edits a caller-authored comment.

### `DELETE /api/v1/comments/{commentId}`
Deletes a caller-authored or admin-authorized comment.

## Upvotes

### `POST /api/v1/ideas/{ideaId}/upvote/toggle`
Toggles the caller's upvote and returns the new upvote state and count.

## Notification Events

Event types:
- `IdeaMentioned`
- `CommentMentioned`
- `IdeaCommented`
- `IdeaStatusChanged`

Event payload shape:
- `eventId`
- `eventType`
- `organizationId`
- `boardId`
- `ideaId`
- `actorUserId`
- `recipientUserId`
- `occurredAtUtc`
- `ideaLink`
- `metadata`
