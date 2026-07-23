# Feature Specification: Boards and Statuses

## Summary
Implement organization-scoped statuses and boards with swimlane ordering, default provisioning, and safe status lifecycle behavior.

## Requirements
- Organizations have default statuses provisioned on creation.
- Statuses are organization-scoped and soft-deleted.
- Boards require at least two swimlanes.
- Boards use subsets of organization statuses.
- Swimlane reorder persists immediately.
- Board views use guided empty states with a primary action and short explanatory text when no ideas are present.
