# Contracts

## Purpose
Defines the system contracts that implementations must follow.

## Route Conventions
- HTTP APIs use path versioning under `/api/v1`.
- Resource routes use plural nouns.
- Nested routes are allowed when needed to express parent-child ownership clearly.

## Error Envelope
- All non-2xx responses use a problem-details-style payload.
- Standard fields are `type`, `title`, `status`, `detail`, and `instance`.
- Validation failures may include an additional `errors` object keyed by field name.

## Standard Error Responses
- `400 Bad Request`: request JSON is malformed, required fields are missing, field values violate contract constraints, or the request shape is otherwise invalid.
- `401 Unauthorized`: the caller is not authenticated or the authentication token is missing, expired, or invalid.
- `403 Forbidden`: the caller is authenticated but not permitted to perform the requested action in the current scope.
- `404 Not Found`: the targeted resource does not exist or is not visible within the caller's authorized scope.

## Collection Conventions
- Organization, user, idea, and comment list endpoints support pagination in MVP.
- Smaller configuration collections such as statuses, boards, and tags may return full result sets unless a feature-specific contract says otherwise.
- Paginated collections support basic filtering plus one explicit sort field and sort direction.
- Archived organizations are hidden from list results by default unless explicitly filtered with `isArchived=true` or an equivalent include-archived flag.

## Update Conventions
- MVP update operations use last-write-wins behavior unless a feature-specific contract defines a stronger rule.

## Validation Ownership
- The API contract validates request shape, required fields, and basic field constraints.
- The Application and Domain layers enforce business rules, authorization rules, and cross-entity invariants.

## Shared Data Rules
- Identifiers are GUID strings.
- Timestamps are UTC ISO-8601 strings.
- Enum values are serialized as strings.
- Paginated responses use:
	- `items`
	- `page`
	- `pageSize`
	- `totalCount`
	- `sortBy`
	- `sortDirection`
- User and organization text fields are trimmed before validation and persistence.
- First and last name maximum length is 100 characters.
- Company name maximum length is 200 characters.
- Address maximum length is 200 characters.
- City maximum length is 100 characters.
- State maximum length is 50 characters.
- Zip maximum length is 20 characters.
- Phone maximum length is 25 characters.
- Comment body maximum length is 2000 characters and supports plain text with line breaks only.

## Authentication Contracts

### `POST /api/v1/auth/login`
Purpose: Authenticate a user with globally unique email credentials.

Request body:
- `email` required string
- `password` required string

Success response `200` authenticated:
- `accessToken` string
- `expiresInSeconds` integer
- `requiresPasswordChange` boolean
- `user`
	- `userId` GUID string
	- `organizationId` GUID string or `null` for Site Admin
	- `role` string
	- `firstName` string
	- `lastName` string
	- `email` string
	- `status` string

Error responses:
- `401` invalid credentials
- `403` inactive account
- `429` locked out after 5 failed attempts within 15 minutes

### `GET /api/v1/auth/me`
Purpose: Return the currently authenticated user summary.

Success response `200`:
- `userId`
- `organizationId`
- `role`
- `firstName`
- `lastName`
- `email`
- `status`

Error responses:
- `401` caller is not authenticated

### `POST /api/v1/auth/change-password`
Purpose: Change the current user's password, including the first-login Site Admin password change.

Request body:
- `currentPassword` required string
- `newPassword` required string

Success response:
- `204 No Content`

Error responses:
- `400` invalid password policy
- `401` invalid current password
- `403` caller is authenticated but not allowed to change the password in the current state

### `POST /api/v1/users/{userId}/temporary-password`
Purpose: Later-phase admin-issued temporary password reset.

Request body:
- empty body or implementation-defined admin note

Success response `200`:
- `temporaryPassword` string
- `mustChangePassword` boolean

Behavior rules:
- the temporary password is displayed one time only
- the temporary password expires after 24 hours if unused
- the user must change the password on first successful use

## Organization Contracts

### `GET /api/v1/organizations`
Purpose: List organizations for Site Admin with pagination.

Query parameters:
- `page`
- `pageSize`
- `search` optional
- `isArchived` optional boolean
- `sortBy` optional `companyName` or `createdAt`
- `sortDirection` optional `asc` or `desc`

Success response `200` paged item shape:
- `organizationId`
- `companyName`
- `city`
- `state`
- `phone`
- `isArchived`

Default list behavior:
- archived organizations are excluded unless explicitly filtered in

Error responses:
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to list organizations

### `POST /api/v1/organizations`
Purpose: Create an organization and provision default statuses plus one default board.

Request body:
- `companyName` required string
- `address` required string
- `city` required string
- `state` required string
- `zip` required string
- `phone` required string
- `primaryContactFirstName` required string
- `primaryContactLastName` required string

Success response `201`:
- `organizationId`
- `defaultBoardId`
- `defaultStatusCount`

Error responses:
- `400` request body is malformed or violates field constraints
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to create organizations

### `GET /api/v1/organizations/{organizationId}`
Purpose: Return organization detail.

Error responses:
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to view this organization
- `404` organization does not exist or is outside caller scope

### `PUT /api/v1/organizations/{organizationId}`
Purpose: Update organization detail.

Request body:
- same fields as organization create

Success response:
- `200` updated organization detail

Error responses:
- `400` request body is malformed or violates field constraints
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to update this organization
- `404` organization does not exist or is outside caller scope

### `POST /api/v1/organizations/{organizationId}/archive`
Purpose: Archive an organization without hard deletion.

Success response:
- `204 No Content`

Error responses:
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to archive this organization
- `404` organization does not exist or is outside caller scope

## User Contracts

### `GET /api/v1/organizations/{organizationId}/users`
Purpose: List users within an organization with pagination.

Query parameters:
- `page`
- `pageSize`
- `search` optional
- `role` optional
- `status` optional `Active` or `Inactive`
- `sortBy` optional `lastName`, `email`, or `createdAt`
- `sortDirection` optional `asc` or `desc`

Success response `200` paged item shape:
- `userId`
- `organizationId`
- `firstName`
- `lastName`
- `email`
- `role`
- `status`

Error responses:
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to list users in this organization
- `404` organization does not exist or is outside caller scope

### `POST /api/v1/organizations/{organizationId}/users`
Purpose: Create a user within an organization.

Request body:
- `firstName` required string
- `lastName` required string
- `email` required string
- `role` required string
- `initialPassword` required string
- `status` optional, defaults to `Active`

Success response `201`:
- `userId`
- `organizationId`
- `email`
- `role`
- `status`

Error responses:
- `400` request body is malformed or violates field constraints
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to create users in this organization
- `404` organization does not exist or is outside caller scope

### `GET /api/v1/users/{userId}`
Purpose: Return user detail.

Error responses:
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to view this user
- `404` user does not exist or is outside caller scope

### `PUT /api/v1/users/{userId}`
Purpose: Update user profile, role, or status within the caller's authorized scope.

Request body:
- `firstName` required string
- `lastName` required string
- `email` required string
- `role` required string
- `status` required string

Success response:
- `200` updated user detail

Error responses:
- `400` request body is malformed or violates field constraints
- `401` caller is not authenticated
- `403` caller is authenticated but not allowed to update this user
- `404` user does not exist or is outside caller scope

## Status Contracts

### `GET /api/v1/organizations/{organizationId}/statuses`
Purpose: List all active and visible statuses for an organization.

Success response `200` item shape:
- `statusId`
- `organizationId`
- `name`
- `isDeleted`

Historical display behavior:
- when a soft-deleted status is surfaced through related entities, the prior name remains visible with an archived or deleted label

### `POST /api/v1/organizations/{organizationId}/statuses`
Purpose: Create a new organization status.

Request body:
- `name` required string

Success response `201`:
- `statusId`
- `name`

### `PUT /api/v1/statuses/{statusId}`
Purpose: Rename or update a status.

Request body:
- `name` required string

### `DELETE /api/v1/statuses/{statusId}`
Purpose: Soft-delete a status while preserving existing references.

Success response:
- `204 No Content`

## Board Contracts

### `GET /api/v1/organizations/{organizationId}/boards`
Purpose: List boards for an organization.

Success response `200` item shape:
- `boardId`
- `organizationId`
- `name`
- `swimlaneCount`

### `POST /api/v1/organizations/{organizationId}/boards`
Purpose: Create a board with at least two swimlanes.

Request body:
- `name` required string
- `swimlanes` required array of
	- `statusId` GUID string
	- `order` integer

Success response `201`:
- `boardId`
- `name`
- `swimlanes`

### `GET /api/v1/boards/{boardId}`
Purpose: Return board detail including swimlanes.

### `PUT /api/v1/boards/{boardId}`
Purpose: Update board name or selected statuses.

Request body:
- `name` required string
- `swimlanes` required array of `statusId` and `order`

### `POST /api/v1/boards/{boardId}/swimlanes/reorder`
Purpose: Persist swimlane reorder immediately after drag-and-drop.

Request body:
- `swimlanes` required array of
	- `statusId`
	- `order`

Success response:
- `204 No Content`

## Idea Contracts

### `GET /api/v1/boards/{boardId}/ideas`
Purpose: List ideas on a board with pagination.

Query parameters:
- `page`
- `pageSize`
- `search` optional
- `statusId` optional
- `tag` optional
- `sortBy` optional `createdAt`, `updatedAt`, or `upvoteCount`
- `sortDirection` optional `asc` or `desc`

Success response `200` paged item shape:
- `ideaId`
- `boardId`
- `title`
- `statusId`
- `statusName`
- `upvoteCount`
- `authorUserId`
- `createdAtUtc`

### `POST /api/v1/boards/{boardId}/ideas`
Purpose: Create a new idea on a board.

Request body:
- `title` required string, max 150 characters
- `description` required string, max 4000 characters
- `statusId` optional GUID string, defaults to the left-most swimlane when omitted
- `tagNames` optional string array
- `mentionEmails` optional string array

Success response `201`:
- `ideaId`
- `boardId`
- `statusId`
- `title`

### `GET /api/v1/ideas/{ideaId}`
Purpose: Return full idea detail.

Success response `200`:
- `ideaId`
- `boardId`
- `title`
- `description`
- `statusId`
- `statusName`
- `tagNames`
- `mentions`
- `comments`
- `upvoteCount`

### `PUT /api/v1/ideas/{ideaId}`
Purpose: Update idea content.

Request body:
- `title` required string, max 150 characters
- `description` required string, max 4000 characters
- `tagNames` optional string array
- `mentionEmails` optional string array

### `POST /api/v1/ideas/{ideaId}/status`
Purpose: Move an idea to another board status.

Request body:
- `statusId` required GUID string

Success response:
- `204 No Content`

## Comment Contracts

### `GET /api/v1/ideas/{ideaId}/comments`
Purpose: List comments for an idea with pagination and chronological ordering.

Query parameters:
- `page`
- `pageSize`
- `sortBy` fixed to chronological order
- `sortDirection` optional `asc` or `desc`

Success response `200` paged item shape:
- `commentId`
- `ideaId`
- `authorUserId`
- `body`
- `createdAtUtc`
- `updatedAtUtc`

### `POST /api/v1/ideas/{ideaId}/comments`
Purpose: Add a comment to an idea.

Request body:
- `body` required string, max 2000 characters, plain text with line breaks

UX rules:
- clients should show a live character counter and inline overflow validation

Success response `201`:
- `commentId`
- `ideaId`

### `PUT /api/v1/comments/{commentId}`
Purpose: Edit a comment authored by the caller.

Request body:
- `body` required string, max 2000 characters, plain text with line breaks

UX rules:
- clients should show a live character counter and inline overflow validation

### `DELETE /api/v1/comments/{commentId}`
Purpose: Delete a comment authored by the caller or by an authorized admin.

Success response:
- `204 No Content`

## Upvote Contracts

### `POST /api/v1/ideas/{ideaId}/upvote/toggle`
Purpose: Toggle the caller's upvote on an idea.

Success response `200`:
- `ideaId`
- `hasUpvoted` boolean
- `upvoteCount` integer

## Notification Event Contract

### Internal notification event types
- `IdeaMentioned`
- `CommentMentioned`
- `IdeaCommented`
- `IdeaStatusChanged`

### Internal notification event payload
- `eventId` GUID string
- `eventType` string
- `organizationId` GUID string
- `boardId` GUID string
- `ideaId` GUID string
- `actorUserId` GUID string
- `recipientUserId` GUID string
- `occurredAtUtc` UTC timestamp
- `ideaLink` string using `/org/{organizationId}/boards/{boardId}/ideas/{ideaId}`
- `message` human-readable event summary string
- `metadata` object for event-specific context

MVP event query scope:
- audit and notification events must be persisted for internal processing and verification
- read or query endpoints for those events are not required in MVP
- verification should be provided through tests and internal diagnostics outside the public API surface

## Notes
- API routes, request/response schemas, and validation rules should be defined here.
- OpenAPI documents should stay aligned with this file.