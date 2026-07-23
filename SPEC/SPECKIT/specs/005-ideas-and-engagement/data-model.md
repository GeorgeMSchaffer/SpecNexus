# Data Model: Ideas and Engagement

## Idea
- `ideaId`
- `boardId`
- `organizationId`
- `authorUserId`
- `title`
- `description`
- `statusId`

## Tag
- `tagId`
- `organizationId`
- `name`
- `normalizedName`

## Comment
- `commentId`
- `ideaId`
- `authorUserId`
- `body`
- `createdAtUtc`
- `updatedAtUtc`

## Upvote
- `ideaId`
- `userId`
