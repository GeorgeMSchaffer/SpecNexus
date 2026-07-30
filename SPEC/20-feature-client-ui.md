## NAVIGATION

-/Home: Displays a list of various boards the user has access to as well a dashboard
- /Admin: This is the top level page for admin tasks such as managing and creating organizations, users, status and all other admin only activities.  Each activity should have it's own page with the home page showing links to pages.
    /admin/organization :  Create and Manage organizations
    /admin/organization/{orgId}/users: Create and Manage Users for an org
    /admin/organization/{orgId}/statuses:  Create and manage statuses for the org

- /boards/{orgId}/board/{boardId}/ :

## VISUAL DESIGN DIRECTION (Selected 2026-07-30)

Comp A "Command Center" (`SPEC/mockups/comp-a-command-center.html`) is the selected UI/UX layout direction for all client pages, restyled with the typography and color palette established in the SVG mockup set (`SPEC/mockups/01-login-and-org-selection.svg` through `12-idea-card-and-overlay.svg`).

### Layout (from Comp A)
- App shell: 48px top bar (logo, global search, primary "+ New idea" action, notifications, avatar) plus a 240px persistent left navigation rail with grouped sections (Workspace / Boards / Administration) and an accent inset marker on the active item.
- Content pages use breadcrumbs, a page title with short subtitle, and a command bar (primary action, filters) above dense data tables or cards.
- Home is a dashboard: KPI stat cards plus "Your boards" table and a recent-activity feed.
- Admin hub uses link cards per tool (Organizations, Users, Statuses) plus a recent admin activity table.
- Board view uses swimlane columns of compact idea cards (title, priority, tags, assignee, upvote) with a priority edge accent; clicking a card title opens the idea detail overlay.
- Idea detail is a centered overlay with a two-pane body: content/comments on the left, metadata sidebar (status, priority, assignee, due date, tags, audit info) on the right.
- Auth screens (login, first-login password change) are centered cards on a navy-to-blue gradient background.
- Implement with Fluent UI Blazor components per `SPEC/mockups/README.md` implementation notes (providers, dialog/toast services, no manual asset tags).

### Typography
- Font family: `"Segoe UI", Arial, sans-serif` for all text; hierarchy is carried by weight and size, not by additional families.

### Color palette (from SVG mockups)
- Ink/neutrals: text `#0f172a`, secondary `#334155` / `#475569`, muted `#64748b` / `#94a3b8`.
- Surfaces: background `#f8fafc`, cards `#ffffff`, subtle fills `#f1f5f9`, borders `#d0d7de` / `#cbd5e1` / `#e2e8f0`.
- Brand/accent: primary blue `#1d4ed8`, deep navy `#1e3a8a` (hover, emphasis, auth gradient), soft accents `#dbeafe` / `#eff6ff` / `#93c5fd`.
- Semantic: success `#166534` on `#dcfce7`; warning `#9a3412` on `#fff7ed`; error `#7f1d1d` on `#fef2f2`.

## ERROR DISPLAY

- Frontend error surfaces must show the full underlying error message in Development.
- Frontend error surfaces must show a generic user-safe message in Production.
- The same UI should remain available in both modes, but the content should differ based on the runtime environment.