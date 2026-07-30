# Feature: Ideas and Engagement

## Outcome
Users can create, discuss, organize, and support ideas within their organization.

## Idea Rules
1. A board can contain zero or more ideas.
2. Each idea must include:
   - Title (required, max 150 characters)
   - Description (required, max 4000 characters)
   - Priority (required): `Low`, `Medium`, `High`, or `Critical`
   - Due Date (optional)
   - Status (required) this is set by which Swim Lane the idea is in.  The default is the left most lane.
   - Assigned To (optional)
   - Tags
   - Mentions
   - Comments
   - Number of Upvotes
3. Board cards must remain compact and display only:
   - Title
   - Priority
   - Assigned To
   - Upvote icon and count
4. Clicking the idea title from a board card opens a detail overlay for full idea review and editing without leaving the board page.
5. The detail overlay must support all editable idea fields and collaboration fields, including tags, mentions, due date, assignment, comments, and upvote state.
6. Ideas in the `Complete` status remain editable and continue to allow comments, mentions, and upvotes.
7. Idea creation, edits, status changes, comments, and upvote toggles must generate audit events.
8. In Development, seeded demo boards include example ideas whose description fields contain sample spec-style detail text.

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
8. Development startup seed includes example comments on seeded ideas for collaboration walkthroughs.

## Upvotes
1. All authenticated users, including Read Only, can upvote ideas.
2. Upvoting is a toggle.
3. A user can have at most one active upvote per idea.
4, Upvotes are counted per Idea and displayed next to the upvote icon.
5. Only the user who cast an upvote can remove it.

## Approval Workflow Decisions
The following implementation decisions were clarified for any future approval or review workflow around ideas:

- Approval is modeled as a workflow state transition, not as a separate entity.
- The idea remains editable while pending approval, but only authorized roles can move it to the next state.
- A pending approval request expires after 24 hours and automatically returns to the previous state if no action is taken.
- Rejection returns the idea to the last non-terminal state and preserves the existing revision history.
- Concurrent edits are resolved with last-write-wins semantics, and the latest approved state wins on save.
- AI-generated or AI-assisted content is treated as untrusted until reviewed by a human; the system must not auto-approve AI-generated submissions.

## Acceptance Criteria
- [ ] Required idea fields are enforced
- [ ] Idea title is limited to 150 characters
- [ ] Idea description is limited to 4000 characters
- [ ] Idea priority is required and limited to `Low`, `Medium`, `High`, or `Critical`
- [ ] Idea due date is optional
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
- [ ] Board cards only show title, priority, assigned-to, and upvote icon/count
- [ ] Clicking an idea title from a board card opens a detail overlay instead of navigating away
- [ ] The detail overlay supports all idea edit fields including tags, mentions, assignment, and optional due date
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
- [ ] Development startup seed provides example ideas with description-based spec content
- [ ] Development startup seed provides example comments on seeded ideas