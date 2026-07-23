# Feature Specification: Ideas and Engagement

## Summary
Implement ideas, tags, mentions, comments, and upvotes within organization-scoped collaboration rules.

## Requirements
- Ideas require title and description.
- Title max length is 150 and description max length is 4000.
- Tags are normalized, unique within an organization, and created on save by users who can edit ideas.
- Mentions resolve by email in ideas and comments.
- Unresolved mentions show inline validation and block save until corrected or removed.
- Read Only users can comment and upvote but cannot edit ideas.
- Comment authors can edit and delete their own comments.
- Comment entry uses a live character counter and inline overflow validation for the 2000-character plain-text rule.
- Upvotes are toggled and owned by the user who cast them.
- Completed ideas remain collaborative.
- Idea lifecycle actions are audited.
