# Feature Specification: Boards and Statuses

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/10-requirements.md`
	- `SPEC/20-feature-boards-and-statuses.md`
	- `SPEC/20-feature-client-ui-revisions.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-07-31

## Summary
Implement organization-scoped statuses and boards with swimlane ordering, default provisioning, and safe status lifecycle behavior.

## Requirements
- Organizations have default statuses provisioned on creation.
- Statuses are organization-scoped and soft-deleted.
- Boards require at least two swimlanes.
- Boards use subsets of organization statuses.
- Swimlane reorder persists immediately.
- In Development only, seeded demo organizations include one example board populated with ideas across each default swimlane.
- Board views use guided empty states with a primary action and short explanatory text when no ideas are present.
- The Workflow page displays only a list of boards the user can access (columns: Name, Board Type, Status); clicking a board navigates to that board's swimlane view.
- The Settings → Boards & Statuses admin page shows a boards list (Name, Board Type, Status) and a statuses list (Name, Color, Sort Order, Is Default). This requires domain additions: `Board.IsArchived`, `Status.Color`, `Status.SortOrder`, `Status.IsDefault`.
