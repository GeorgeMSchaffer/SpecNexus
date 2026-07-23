# Feature: Ideas and Engagement

## Outcome
Users can create, discuss, organize, and support ideas within their organization.

## Idea Rules
1. A board can contain zero or more ideas.
2. Each idea must include:
   - Title (required, max 150 characters)
   - Description (required, max 4000 characters)
   - Status (required) this is set by which Swim Lane the idea is in.  The default is the left most lane.
   - Tags
   - Mentions
   - Comments
   - Number of Upvotes
3. Ideas in the `Complete` status remain editable and continue to allow comments, mentions, and upvotes.
4. Idea creation, edits, status changes, comments, and upvote toggles must generate audit events.

## Permissions
- Site Admin, Org Admin, and User can create and edit ideas.
- Read Only cannot edit idea content.
- User can update idea status for any idea on a board if allowed by board configuration.

## Tags
1. Tags are scoped to the organization.
2. Users who can edit ideas can create new tags.
3. Users can select existing tags or create new ones up to 100 characters.
4. Tag autocomplete begins after 2 entered characters.
5. If no match exists, the new tag is created when the idea is saved.
6. Tags are trimmed, compared case-insensitively, and must be unique within an organization.
7. If concurrent saves attempt to create the same normalized tag, the system merges them into a single tag.

## Mentions
1. Users can mention other users in their organization using the `@` trigger and an email-based lookup.
2. Mention suggestions are limited to users in the same organization.
3. Mentions are resolved to the matching user when the idea or comment is saved.
4. If a typed mention does not resolve to a same-organization user, the UI must show inline validation and block save until the unresolved mention is removed or corrected.

## Comments
1. All authenticated users, including Read Only, can comment on ideas.
2. Comments are displayed chronologically.
3. Comment authors can edit and delete their own comments.
4. Site Admin and Org Admin can delete any comment within their authorized scope.
5. Comments support the same email-based mention behavior as ideas.
6. Comment bodies are plain text, may include line breaks, and are limited to 2000 characters.
7. Comment entry shows a live character counter and inline validation when the maximum length is exceeded.

## Upvotes
1. All authenticated users, including Read Only, can upvote ideas.
2. Upvoting is a toggle.
3. A user can have at most one active upvote per idea.
4, Upvotes are counted per Idea and displayed next to the upvote icon.
5. Only the user who cast an upvote can remove it.

## Acceptance Criteria
- [ ] Required idea fields are enforced
- [ ] Idea title is limited to 150 characters
- [ ] Idea description is limited to 4000 characters
- [ ] Tag autocomplete begins after 2 characters
- [ ] Tag values are limited to 100 characters
- [ ] New tags can be created on save
- [ ] Tags are trimmed, case-insensitive, and unique within an organization
- [ ] Concurrent creation of the same normalized tag results in a single shared tag
- [ ] Read Only users cannot create new tags because they cannot edit ideas
- [ ] Mention lookup resolves users by email within the same organization
- [ ] Mentions in comments resolve users by email within the same organization
- [ ] Unresolved mentions show inline validation and block save until corrected or removed
- [ ] Ideas in `Complete` status remain editable and collaborative
- [ ] Mentions are restricted to users in the same organization
- [ ] Comment authors can edit and delete their own comments
- [ ] Site Admin and Org Admin can delete comments in their authorized scope
- [ ] Comment bodies are plain text with line breaks and are limited to 2000 characters
- [ ] Comment entry shows a live character counter and inline overflow validation
- [ ] When enabled by board configuration, Users can update the status of any idea on that board
- [ ] Read Only can comment and upvote
- [ ] Upvoting toggles on second click
- [ ] Only the user who cast an upvote can remove it
- [ ] Idea lifecycle actions generate audit events