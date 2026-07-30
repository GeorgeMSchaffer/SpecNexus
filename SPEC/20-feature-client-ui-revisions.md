# Feature: Client UI Revisions (Bugs and Tweaks)

## Purpose

Defines a batch of client UI bug fixes and structural revisions covering layout, navigation, the Settings (formerly Admin) area, list-page conventions, and removal of template placeholder code. Decisions were captured via QA interview on 2026-07-30.

## Bug Fixes

### BUG-1: Errant `else {` on Change Password screen
- The Change Password page renders a literal `else {` as visible page content.
- Fix the malformed Razor conditional so no code text is displayed.
- Acceptance: no stray code fragments render on `ChangePassword` in any state (initial, validation error, success).

### BUG-2: Placeholder template code removal
- Remove all Weather/forecast and Counter placeholder functionality left over from the Blazor template:
  - `Pages/Weather.razor`, `Pages/Counter.razor`, related sample data/services, and any nav links to them.
- Acceptance: no routes, menu items, or code references to Weather or Counter remain; solution builds and tests pass.

## Layout and Header

### Header
- Header background color is `rgb(33, 37, 41)` across the entire header bar.
- The signed-in Username in the header renders in white.
- Sign Out is an icon button placed immediately to the LEFT of the Username display.
- A gear icon in the header navigates to the Settings area (see below). The gear is visible to all authenticated users.

### Menu
- The primary menu is horizontal and sits directly under the main header (no vertical sidebar nav).
- Menu items: Home, Workflow, Ideas.
- Admin functionality is NOT in the horizontal menu; it is reached only via the header gear icon.

## Settings Area (formerly "Admin")

- The admin area is renamed to "Settings" everywhere: page titles, gear icon tooltip, and routes (`/settings/...`).
- Clicking the header gear icon navigates to the Settings landing page.
- The Settings landing page shows:
  - My Profile: every authenticated user can view and update their own information here.
  - Role-scoped admin links:
    - Site Admin: Organizations, Users, Boards & Statuses.
    - Org Admin: Users and Boards & Statuses, scoped to their own organization only.
    - Member: My Profile only; no admin links are rendered.
- Link visibility is a UI convenience only; the API remains the authority for authorization.

## Admin-Style Pages: List/Form Pattern

Applies to Settings pages for Organizations, Users, and Boards & Statuses.

- Each admin-style page defaults to a LIST view.
- A Create button (e.g., "Create Organization") appears above the list, visible only to roles permitted to create that entity (Create Organization: Site Admin only).
- Clicking Edit on a row, or Create, swaps the list view for the FORM view on the same page.
- Saving or cancelling the form returns to the list view with the list refreshed.

### Organization list
- Columns: Title, Description, Invite Code, Status (Active/Archived).
- Search filters across exactly those displayed columns.
- Contact/address profile fields (city, state, phone, company) are not list columns and are not searched; they remain editable on the organization form.

## Workflow Page

- The Workflow page displays ONLY a list of boards the user can access (no embedded swimlanes or other content).
- Clicking a board navigates to that board's swimlane/kanban view.

## Ideas Page (new)

- New "Ideas" page reachable from the horizontal menu.
- Displays a single combined list of ideas that were created by OR are assigned to the current user.
- A filter control offers: All (default), Created by me, Assigned to me.
- Clicking Details on a row swaps the list for the Edit Idea form inline on the same page (list → form pattern, consistent with admin-style pages).
- Saving or cancelling returns to the list.

## Uniform List Conventions (all list pages)

Applies to Organizations, Users, Ideas, and any future entity list page.

- Uniform search bar rendered above the list.
- Search filters across all properties displayed as list columns for that entity.
- Pagination controls with page size options 25 (default), 50, 100, 250.
- Search and pagination are SERVER-SIDE: list API endpoints accept `search`, `page`, and `pageSize` query parameters per SPEC/30-Contracts.md collection conventions.
- Changing the search text resets to page 1.

## Acceptance Criteria

- [ ] Change Password page renders no stray `else {` text.
- [ ] Weather and Counter pages, links, and code are fully removed.
- [ ] Header uses `rgb(33, 37, 41)`; Username is white; Sign Out icon sits left of the Username.
- [ ] Horizontal menu under the header shows Home, Workflow, Ideas only.
- [ ] Gear icon navigates to `/settings`; area is titled "Settings" everywhere.
- [ ] Settings landing shows My Profile for all users and role-correct admin links (Site Admin: Orgs/Users/Boards & Statuses; Org Admin: Users/Boards & Statuses own-org; Member: none).
- [ ] Admin-style pages default to list view; Create Organization button is visible only to Site Admins; Edit/Create swaps to form view and returns to list on save/cancel.
- [ ] Workflow page shows only a board list; clicking a board opens its swimlane view.
- [ ] Ideas page lists created-by-me and assigned-to-me ideas with All/Created/Assigned filter; Details swaps to inline Edit Idea form.
- [ ] All list pages have a uniform search bar and server-side pagination with 25/50/100/250 page sizes (default 25); org list searches Title, Description, Invite Code, Status.
