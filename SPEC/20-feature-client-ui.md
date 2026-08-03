## NAVIGATION

Primary navigation is a horizontal menu under the main header: Home, Workflow, Ideas. The admin area (renamed "Settings") is reached via a gear icon in the header, not the menu. See SPEC/20-feature-client-ui-revisions.md for layout and Settings details.

- /Home: Displays a list of various boards the user has access to as well a dashboard
- /workflow: List of boards the user can access; clicking a board opens its swimlane view
- /ideas: Kanban board showing ideas as cards in swimlane columns grouped by the selected board's statuses
- /settings: Settings landing page (formerly Admin) with My Profile and role-scoped admin links.
    /settings/organizations :  Create and Manage organizations (list-first, form on create/edit)
    /settings/organizations/{orgId}/users: Create and Manage Users for an org
    /settings/organizations/{orgId}/statuses:  Create and manage statuses for the org

- /boards/{orgId}/board/{boardId}/ : Board swimlane view

## /ideas — Kanban Board

Route: `/ideas`

Replaces the previous flat-list design (All / Created by me / Assigned to me filter tabs and inline edit form). Ideas are displayed as cards arranged in swimlane columns, where each column represents one `Status` from the selected board.

### Board Picker
- A dropdown in the page header lets the user select which board to view. Default = first board returned by the API.
- Board selection is persisted in `localStorage` (key: `ideas-board-id`).
- Changing the board reloads statuses and ideas.

### Board Header
The board header contains: board name (left), board picker dropdown, search input (placeholder: "Search title, tag, assignee"), and a primary **New Idea** button (right). The New Idea button opens the detail overlay in create mode, pre-populating the target status as the left-most column. The New Idea button is hidden for ReadOnly users.

### Swimlane Columns
- One column per `Status` on the selected board, ordered by `Status.SortOrder` ascending.
- Column header shows the status name, a colour dot (`Status.Color`), and idea count.
- Columns scroll horizontally if they overflow the viewport.

### Idea Cards
Cards are compact. Each card shows: title (clickable, 2-line truncation), priority badge, assignee name, and upvote count. Clicking the card title opens an in-context detail overlay — no page navigation. The overlay supports full editing: title, priority, due date (optional), description, assignee, tags, mentions, and comments. Overlay actions: **Cancel**, **Save Idea**, **Move in Board** (status picker without dragging).

### Filter Chips
Filter chips appear above the board: **All**, **Created by me** (`AuthorUserId == currentUserId`), **Assigned to me** (`AssigneeUserId == currentUserId`). Filtering is client-side. Empty columns remain visible with a "No ideas" placeholder.

### Search
Text input above the board filters cards by title, tag, or assignee (client-side, case-insensitive). Combinable with filter chips.

### Drag-and-Drop: Moving an Idea
1. User drags a card to another column.
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

### Component Structure
```
Ideas.razor                    ← page shell
  └─ IdeaKanbanBoard.razor     ← horizontal scroll + column drag
       └─ KanbanColumn.razor   ← column header, drop zone, card list
            └─ IdeaCard.razor  ← card, drag source
```
New components live in `src/SargentNexus.Client/Shared/Kanban/`.

### DnD Technology
HTML5 drag-and-drop (desktop only). Touch/mobile drag is deferred; mobile view is scrollable.

### Acceptance Criteria
- `/ideas` shows a Kanban board, not a list
- Board picker defaults to first board; selection persists in `localStorage`
- Columns reflect the selected board's statuses in `SortOrder` order
- Filter chips and title/tag/assignee search work across all columns (client-side)
- Card drag calls `MoveIdeaStatusAsync`; idea status set to target swimlane's status; reverts on failure with toast
- Column drag (SiteAdmin/OrgAdmin only) saves immediately on drop; calls `UpdateStatusAsync` per affected status; reverts on failure
- Clicking a card title opens the in-context detail overlay; no page navigation occurs
- Detail overlay provides Cancel, Save Idea, and Move in Board actions
- New Idea button in board header opens overlay in create mode; hidden for ReadOnly users
- Mobile/touch: scrollable view, no drag support

## ERROR DISPLAY

- Frontend error surfaces must show the full underlying error message in Development.
- Frontend error surfaces must show a generic user-safe message in Production.
- The same UI should remain available in both modes, but the content should differ based on the runtime environment.