# Feature: Boards and Statuses

## Outcome
Organizations can manage idea boards using configurable workflow swimlanes.

## Status Rules
1. Statuses are defined at the organization level.
2. Site Admin and Org Admin can create, edit, and delete statuses.
3. Default statuses are:
   - New / Pending
   - In Review
   - In Progress
   - Client Review
   - Complete
4. The default statuses are provisioned automatically when a new organization is created.
5. Status deletion is soft-delete only so existing board and idea references remain valid.
6. Historical or detail views that reference a soft-deleted status must continue to show the prior status name with an archived or deleted label.

## Board Rules
1. A board is a collection of ideas organized by swimlanes.
2. Each swimlane maps to a status.
3. A board must have at least 2 swimlanes.
4. Each new organization starts with one default board.
5. Site Admin and Org Admin can:
   - select statuses used by a board
   - reorder swimlanes by drag-and-drop
6. Swimlane order changes are saved immediately when the drag-and-drop action completes.
7. Board views must provide guided empty states with a primary action and short explanatory text when no ideas exist.

## Acceptance Criteria
- [ ] Organization-scoped statuses can be created and maintained
- [ ] A new organization receives the default status set automatically
- [ ] Deleting a status performs a soft delete so existing references remain valid
- [ ] Historical or detail views show soft-deleted status names with an archived or deleted label
- [ ] A board cannot be created with fewer than 2 swimlanes
- [ ] A new organization receives one default board
- [ ] Boards can select a subset of org statuses
- [ ] Swimlane order can be changed and is saved immediately
- [ ] Board screens provide guided empty states with a primary action and short explanatory text when no ideas exist