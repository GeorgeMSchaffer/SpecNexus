## NAVIGATION

Primary navigation is a horizontal menu under the main header: Home, Boards, Ideas. The admin area (renamed "Settings") is reached via a gear icon in the header, not the menu. See SPEC/20-feature-client-ui-revisions.md for layout and Settings details.

The active item in the application's primary navigation has a flat rectangular background with stronger text and icon color. It has no border radius and no left-edge active border. The active link exposes `aria-current="page"` and retains a visible keyboard focus outline. Tabs, pivots, filter chips, and segmented controls keep their control-specific selected styles.

- /Home: Displays a list of various boards the user has access to as well a dashboard
- /boards: List of boards the user can access; clicking a board opens its swimlane view
- /board/{boardId}: Board detail with ideas arranged in swimlane columns mapped to the board's statuses
- /ideas: User-focused idea list/search surface; it does not duplicate the selected board detail route
- /settings: Settings landing page (formerly Admin) with My Profile and role-scoped admin links.
  /settings/profile : Edit the current user's first and last name and change their password; email and role are read-only.
    /settings/organizations :  Create and Manage organizations (list-first, form on create/edit)
    /settings/organizations/{orgId}/users: Create and Manage Users for an org
    /settings/organizations/{orgId}/statuses:  Create and manage statuses for the org
    /settings/organizations/{orgId}/idea-fields: Create and manage Idea Type and Business Impact options for the org

  - Compatibility redirects: `/board` → `/boards`; `/workflow` and `/workflows` → `/boards`; `/workflow/{boardId}` → `/board/{boardId}`

  ## `/board/{boardId}` — Kanban Board

  Route: `/board/{boardId}`

Replaces the previous flat-list design (All / Created by me / Assigned to me filter tabs and inline edit form). Ideas are displayed as cards arranged in swimlane columns, where each column represents one `Status` from the selected board.

### Board Header
The board header contains: board name (left), search input (placeholder: "Search title, tag, assignee"), and a primary **New Idea** button (right). The New Idea button opens the detail overlay in create mode, pre-populating the target status as the left-most column. The New Idea button is hidden for ReadOnly users.

### Swimlane Columns
- One column per `Status` on the selected board, ordered by `Status.SortOrder` ascending.
- Column header shows the status name, a prominent rule using `Status.Color`, and idea count. A secondary dot may be used only when needed for non-color identification; status name and count remain the accessible identifiers.
- Columns scroll horizontally if they overflow the viewport.

### Idea Cards
Cards are compact. Each card shows: title (clickable, 2-line truncation), priority badge, Business Impact color chip, assigned tags, assigned-user personas, submission age, upvote icon button/count, and Add Comment icon button/count. A dedicated drag handle starts card movement; interactive controls never start a drag. Clicking the card title opens an in-context detail overlay with no page navigation. The overlay supports full editing: title, priority, Idea Type, Business Impact, due date (optional), description when authorized, zero to five assignees, zero to 10 tags, mentions, and comments. Overlay actions include **Cancel**, **Save Idea**, **Move in Board** (status picker without dragging), and an admin-only **Delete Idea** action with confirmation.

Tags are selected and created through a searchable multi-value Tag field in Idea Detail. Anyone authorized to edit the idea can create a reusable organization-scoped tag inline. Tag names are trimmed and matched case-insensitively. Cards display the first three tags alphabetically and a `+N` indicator for the remainder; the complete tag list is available in Idea Detail and accessible text or a keyboard-accessible tooltip.

The Assignees field is an optional searchable multi-select populated with active users from the idea's organization. It accepts at most five distinct users. Inactive users already assigned remain visible but cannot be newly selected. Cards order assignees by first name then last name and show the first three personas followed by `+N`. Each visible persona contains a circular avatar with the first letter of the first name and first letter of the last name, followed by the first name. Full names are available in Idea Detail and accessible text or a keyboard-accessible tooltip. If one name part is unexpectedly absent, use the available initial and first available display label; if both are absent, render `?` with accessible label `Unknown user`.

Cards show submission age as viewer-local calendar-day difference between `createdAtUtc` and today: `0 days ago`, `1 day ago`, or `{N} days ago`. Future values are clamped to `0 days ago`.

The upvote icon is unfilled when inactive and filled when the current user has upvoted. Toggle the icon and count optimistically and restore both on failure. Clicking Add Comment opens the overlay, scrolls comments into view, and focuses the comment composer. If commenting is unavailable, focus the comments heading.

### Filter Chips
Filter chips appear above the board: **All**, **Created by me** (`AuthorUserId == currentUserId`), **Assigned to me** (the current user is in the idea's assignee collection). Filtering is client-side. Empty columns remain visible with a "No ideas" placeholder.

### Search
Text input above the board filters cards by title, tag, or assignee (client-side, case-insensitive). Combinable with filter chips.

### Drag-and-Drop: Moving an Idea
1. User drags a card by its dedicated handle to another column on desktop.
2. Optimistic UI moves the card immediately.
3. Calls `POST /api/v1/ideas/{ideaId}/status` with the target status ID. The idea's status is set to the target swimlane's `Status`.
4. On failure: card reverts, error toast shown.
5. Board `allowUserStatusUpdate` and role restrictions are enforced server-side (403 → revert + permission message).

### Drag-and-Drop: Reordering Columns
1. SiteAdmin and OrgAdmin users can drag column headers to reorder columns.
2. Optimistic reorder applied immediately.
3. For each column whose `SortOrder` changed, calls `PUT /api/v1/boards/{boardId}/statuses/{statusId}` with the new `sortOrder`. Reorder saves immediately on drop — no additional confirmation required.
4. On failure: revert all columns, show error toast.
5. User and ReadOnly roles see columns but cannot reorder them.

6. Changing status in Idea Detail uses the same move operation and immediately relocates the card while the overlay remains open.
7. Keyboard and touch users move ideas with the Idea Detail status selector; touch drag is deferred.

### Component Structure
```
Ideas.razor                    ← page shell
  └─ IdeaKanbanBoard.razor     ← horizontal scroll + column drag
       └─ KanbanColumn.razor   ← column header, drop zone, card list
            └─ IdeaCard.razor  ← card, drag source
```
New components live in `src/SargentNexus.Client/Shared/Kanban/`.

### DnD Technology
HTML5 drag-and-drop (desktop only) with a dedicated handle and visible drop targets. Touch/mobile drag is deferred; mobile view is scrollable and status movement remains available through Idea Detail.

### Acceptance Criteria
- `/boards` shows the board list and `/board/{boardId}` shows the selected board's Kanban view
- `/board`, `/workflow`, `/workflows`, and `/workflow/{boardId}` redirect to canonical Board routes
- No user-facing UI displays Workflow or Workflows terminology
- Columns reflect the selected board's statuses in `SortOrder` order
- Filter chips and title/tag/assignee search work across all columns (client-side)
- Card drag starts only from the dedicated handle, calls `MoveIdeaStatusAsync`, sets the idea status to the target swimlane's status, and reverts on failure with a toast
- Column drag (SiteAdmin/OrgAdmin only) saves immediately on drop; calls `UpdateStatusAsync` per affected status; reverts on failure
- Clicking a card title opens the in-context detail overlay; no page navigation occurs
- Detail overlay provides Cancel, Save Idea, Move in Board, and authorized soft-delete actions
- Changing status in Idea Detail immediately moves the visible card to the matching swimlane
- Cards display Business Impact, current-user upvote state/count, and comment count
- Idea Detail provides a searchable Tag multi-select that selects existing organization tags and creates normalized reusable tags inline for authorized editors, with at most 10 tags per idea
- Idea Detail provides an optional searchable Assignees multi-select containing active users from the idea's organization, with at most five distinct assignees and historical inactive-assignee display
- Cards display the first three tags alphabetically and first three assignee personas by first/last name, with `+N` overflow and complete accessible values
- Each persona shows first-name/last-name initials followed by first name, with an accessible missing-name fallback
- Cards display viewer-local calendar-day submission age with zero, singular, plural, and future-timestamp behavior
- Clicking the card comment action focuses the overlay comment composer
- Upvote toggles optimistically and rolls back on failure
- New Idea button in board header opens overlay in create mode; hidden for ReadOnly users
- Mobile/touch: scrollable view, no drag support, status movement available in Idea Detail

## VISUAL DESIGN DIRECTION (Selected 2026-08-09)

Comp 06 "Jira + Editorial Board" (`SPEC/mockups/ideas-boards-comp-06-jira-editorial-board.html`) is the selected UI/UX look-and-feel authority for the authenticated client workspace. It supersedes Comp A "Command Center" and the sprint-management board artifact as implementation references. Functional, authorization, accessibility, and data requirements in the canonical feature specs take precedence when the static comp omits a state or control.

Comp 06 is exact visual authority for the horizontal app shell, Boards list, Board detail, Ideas list, and Add New Idea presentation. Pages not shown in Comp 06 reuse its design tokens, spacing, controls, and page hierarchy while retaining the workflow-specific requirements and supporting SVG references in `SPEC/20-feature-client-ui-revisions.md`.

### App Shell and Brand
- Use a 52px deep-navy header with the uppercase `SARGENTNEXUS` wordmark; render `NEXUS` in the primary light-blue accent.
- Primary navigation is horizontal in the header and contains Home, Boards, and Ideas. Keep Settings behind the gear icon and retain the authorization behavior defined in `SPEC/20-feature-client-ui-revisions.md`.
- Place global search, Settings, and the signed-in persona at the right side of the desktop header. On narrow screens, hide nonessential header controls and retain the current workspace/page context.
- Content pages use an eyebrow or breadcrumb, concise H1 and supporting text, a right-aligned primary action, and a search/filter command row.
- Use compact rectangular controls and restrained corners. Default control and card radius is 3px to 4px; avoid pill styling except where a semantic chip or persona requires it.

### Boards and Ideas Lists
- `/boards` and `/ideas` use dense, bordered white tables on the light workspace background.
- Table headers use compact uppercase labels, muted text, and a subtle off-white fill. Primary entity links use the strong blue accent.
- Page-heading actions stack to full width on narrow screens. Tables preserve scan-friendly column widths through horizontal overflow; secondary columns may be suppressed only when the same information remains available through the row detail flow.
- Search appears before filter chips. The selected filter uses the soft-blue fill and blue text/border treatment shown in Comp 06.

### Board Detail
- Use the open editorial canvas from Comp 06: lanes sit directly on the page background without filled lane containers or decorative cards around the board.
- The board heading and controls form one composition separated from the lanes by a single ink-colored rule. At intermediate widths, controls wrap beneath the title without overlapping it.
- Lanes are approximately 290px wide, remain a stable width, and scroll horizontally. Each lane header uses a 4px rule bound to `Status.Color`, plus status name and idea count.
- Idea cards are white with a subtle neutral border, minimal shadow at rest, 3px corners, and enough internal spacing to distinguish title, chips, and metadata. Hover/focus strengthens the primary-blue border and may add a restrained shadow.
- Preserve all card content and interactions defined in this specification, including the dedicated drag handle, priority, Business Impact, tags, assignees, age, upvote, comment action, and title-opened detail overlay. The comp controls appearance, not feature scope.
- On mobile, show one lane at approximately 85vw with horizontal snap scrolling. Touch users move status through Idea Detail rather than drag-and-drop.

### Add New Idea
- Creation uses the Comp 06 split composition on wide screens: contextual guidance and a live card preview on the left, with the structured form in a white right-side panel.
- On narrow screens, remove the contextual preview pane and present the form as the primary full-width task surface.
- Keep visible labels, required-field and validation feedback, the starting-status notice, and explicit Cancel and Create Idea actions.

### Typography
- Font family: `"IBM Plex Sans", "Segoe UI", sans-serif` for all client text.
- Use weight and size for hierarchy; do not introduce a separate display or editorial typeface.
- Default body text is approximately 14px with a 1.4 line height. Page titles are compact rather than hero-sized.

### Color and Surface Tokens
- Header/deep navy: `#172b4d`; switcher/reference dark navy: `#0b1f3a`.
- Primary blue: `#0c66e4`; primary hover/strong link: `#0055cc`; light blue brand accent: `#579dff`; soft selected fill: `#e9f2ff`.
- Workspace background: `#f7f8f9`; card/surface: `#ffffff`; primary ink: `#172b4d`; muted text: `#626f86`; borders: `#dcdfe4`.
- Semantic references: error `#ae2e24`, warning `#974f0c`, success `#216e4e`. Semantic text and fills must meet WCAG AA contrast and cannot rely on color alone.

### Implementation Boundary
- Implement with Fluent UI Blazor components and services per `SPEC/mockups/README.md`; reproduce the composition and tokens without copying the comp's demo-only switcher or static JavaScript.
- Do not add Comp 05's serif typography, green portfolio shell, publication-style Ideas index, or full-page composer. Comp 06 intentionally combines Comp 01's Jira-inspired shell and forms with Comp 05's open Board composition only.
- Do not reintroduce Comp A's persistent left navigation rail or the sprint artifact's demo pivots, conversion actions, duplicate commands, approval assumptions, or sprint-specific features.

## ERROR DISPLAY

- Frontend error surfaces must show the full underlying error message in Development.
- Frontend error surfaces must show a generic user-safe message in Production.
- The same UI should remain available in both modes, but the content should differ based on the runtime environment.