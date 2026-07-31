# Feature Specification: Ideas and Engagement

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/10-requirements.md`
	- `SPEC/20-feature-ideas-and-engagement.md`
	- `SPEC/20-feature-client-ui-revisions.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-07-31

## Summary
Implement ideas, tags, mentions, comments, and upvotes within organization-scoped collaboration rules. Includes a user-facing My Ideas list page.

## Requirements
- Ideas require title and description.
- Title max length is 150 and description max length is 4000.
- Tags are normalized, unique within an organization, and created on save by users who can edit ideas.
- Mentions resolve by email in ideas and comments.
- Unresolved mentions show inline validation and block save until corrected or removed.
- Read Only users can comment and upvote but cannot edit ideas.
- When board configuration allows it, Users can move any idea on that board.
- Comment authors can edit and delete their own comments.
- Comment entry uses a live character counter and inline overflow validation for the 2000-character plain-text rule.
- Upvotes are toggled and owned by the user who cast them.
- Completed ideas remain collaborative.
- In Development only, seeded demo ideas include description-based sample spec content and example comments.
- Idea lifecycle actions are audited.
- A My Ideas page (`/ideas`) lists ideas created by or assigned to the current user. A filter control offers All (default), Created by me, and Assigned to me. List columns: Title, Created By, Assigned To, Status, Created Date.
- Clicking Details on an idea row navigates to `/ideas/{id}/edit` (dedicated route); that page has a Back button returning to `/ideas`.
