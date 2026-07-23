# Research: Boards and Statuses

## Key Decisions
- statuses are organization-scoped
- status deletion is soft-delete only
- boards require at least two swimlanes
- reorder is persisted immediately

## Risk Areas
- preserving old idea references when a status is deleted
- immediate reorder persistence without accidental permission gaps
