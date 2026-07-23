# Data Model: Boards and Statuses

## Status
- `statusId`
- `organizationId`
- `name`
- `isDeleted`

## Board
- `boardId`
- `organizationId`
- `name`

## BoardSwimlane
- `boardId`
- `statusId`
- `order`
