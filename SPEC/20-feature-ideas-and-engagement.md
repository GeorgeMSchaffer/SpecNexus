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
7. When a `statusId` is provided on idea create or status update, it must correspond to an active swimlane on the idea's target board; providing a status that is not on the board is a validation error.
8. Idea creation, edits, status changes, comments, upvote toggles, and deletions must generate audit events.
9. In Development, seeded demo boards include example ideas whose description fields contain sample spec-style detail text.

## Rich Content and Attachments Direction (Resolved)
1. MVP idea descriptions and comment bodies remain plain text only.
2. Rich text formatting (HTML, Markdown rendering, WYSIWYG controls) is out of MVP scope.
3. File attachments and embedded media are out of MVP scope.
4. URLs may appear as plain text content but are not treated as trusted embedded content.
5. Rich-content and attachment support is deferred to a future post-MVP phase and requires explicit security and storage contracts before implementation.

## Permissions
- Site Admin, Org Admin, and User can create and edit ideas.
- Site Admin and Org Admin can soft-delete ideas within their authorized scope; soft-deleted ideas are excluded from board views and list queries.
- Idea deletion generates an audit event.
- Read Only cannot edit or delete idea content.
- User can update idea status for any idea on a board if allowed by board configuration.

## Site Admin Organization Context
- Site Admin operates in the context of the specific resource being accessed or modified; org-scoped operations (board, idea, status, tag management) use the organization that owns the resource.
- When creating org-scoped resources (e.g., creating a new board), Site Admin must specify the target `organizationId`.
- Site Admin has no organization affiliation and therefore cannot be @mentioned and will not appear in mention lookup results.

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

## CSV Import

### Rules
1. Only Site Admin and Org Admin can upload ideas via CSV to a board.
2. The CSV file must use UTF-8 encoding with a header row.
3. Supported columns:

   | Column       | Required | Constraints                                                                |
   |--------------|----------|----------------------------------------------------------------------------|
   | `Title`      | Yes      | max 150 characters                                                         |
   | `Description`| Yes      | max 4000 characters                                                        |
   | `Priority`   | Yes      | must be `Low`, `Medium`, `High`, or `Critical`                             |
   | `DueDate`    | No       | ISO-8601 date format (`YYYY-MM-DD`); omit or leave blank to skip           |
   | `Status`     | No       | must match a swimlane name on the target board; defaults to leftmost lane  |
   | `AssignedTo` | No       | email address of a user in the same organization                           |
   | `Tags`       | No       | pipe-delimited (`\|`) list of tag values; max 100 characters per tag       |

4. Validation runs against the entire file before any ideas are created. If any row fails validation, the entire upload is rejected and all errors are returned. No partial imports occur.
5. A single upload is limited to 500 data rows. Files exceeding this limit are rejected.
6. If two or more rows within the same CSV share the same `Title` (case-insensitive), the second and any subsequent duplicate rows are validation errors.
7. If a row's `Title` (case-insensitive) already exists as an idea on the target board, that row is silently skipped without error.
8. An unresolved `AssignedTo` email (not an active user in the same organization) is a validation error.
9. An unrecognized `Status` value (not a swimlane on the target board) is a validation error.
10. New `Tags` values that do not yet exist in the organization are created automatically using the same normalization rules as manual tag creation (trimmed, case-insensitive deduplication).
11. The creation phase (after validation passes) runs inside a single database transaction. If any row fails to persist, all created ideas are rolled back.
12. A successful import generates one bulk-import audit event for the upload action, plus one individual audit event per idea created (same event type as manual idea creation). The bulk-import audit event fires even when all rows were skipped (`importedCount: 0`).

### Acceptance Criteria
- [ ] Only Site Admin and Org Admin can access the CSV upload action for a board
- [ ] CSV files exceeding 500 data rows are rejected before processing
- [ ] Validation covers all rows before any ideas are created
- [ ] A file with any invalid row is rejected entirely and all errors are reported
- [ ] `Title`, `Description`, and `Priority` are required per row; missing or blank values are validation errors
- [ ] `Title` is validated to max 150 characters per row
- [ ] `Description` is validated to max 4000 characters per row
- [ ] `Priority` must be one of `Low`, `Medium`, `High`, or `Critical`; unrecognized values are validation errors
- [ ] `DueDate` must be a valid `YYYY-MM-DD` date when provided; invalid formats are validation errors
- [ ] `Status` must match a swimlane name on the target board when provided; unrecognized values are validation errors
- [ ] Ideas with no `Status` value default to the leftmost swimlane of the target board
- [ ] `AssignedTo` must resolve to an active user in the same organization by email; unresolved values are validation errors
- [ ] `Tags` values are pipe-delimited; new tag values are auto-created using existing normalization rules
- [ ] Rows whose `Title` (case-insensitive) already exists on the target board are silently skipped
- [ ] Two or more rows within the same CSV sharing the same `Title` (case-insensitive) are validation errors
- [ ] The creation phase runs inside a single transaction; a persistence failure rolls back all created ideas
- [ ] A bulk-import audit event is generated for the upload action, including when all rows are skipped
- [ ] One individual audit event is generated per idea created, matching the manual idea-creation audit event type

## Approval Workflow Decisions (Post-MVP — Deferred)
The following decisions are captured for a future post-MVP approval workflow feature. **None of these behaviors are implemented in MVP.** No API contracts, data model fields, background scheduler, or acceptance criteria for approval are required for the MVP release.

When this feature is implemented it must address:
- Approval modeled as a workflow state transition, not as a separate entity.
- Only Org Admins and the idea author can initiate or resolve approval actions.
- Site Admin can always initiate or resolve approval actions within any org.
- A pending approval request expires after 24 hours and automatically returns to the previous state if no action is taken (requires a background scheduler).
- Rejection returns the idea to the last non-terminal state; rejection reason and prior state must be persisted.
- Concurrent edit concurrency during a pending approval requires an explicit decision (optimistic concurrency token recommended).
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
- [ ] MVP idea and comment content remains plain text only
- [ ] Rich text, embedded media, and file attachments are excluded from MVP implementation