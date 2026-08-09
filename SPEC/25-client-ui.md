## NAVIGATION

-/Home: Displays a list of various boards the user has access to as well a dashboard
- /register: Anonymous self-registration page. Requires an organization invite code plus profile details and password; an invalid or missing invite code shows a prompt to enter a correct one.
- /Admin: This is the top level page for admin tasks such as managing and creating organizations, users, status and all other admin only activities.  Each activity should have it's own page with the home page showing links to pages.
    /admin/organization :  Create and Manage organizations. The list view displays each organization's invite code.
    /admin/organization/{orgId}/users: Create and Manage Users for an org, including CSV import of users (no invite code required; imported users join this org). The org detail view displays the invite code.
    /admin/organization/{orgId}/statuses:  Create and manage statuses for the org

- /boards/{orgId}/board/{boardId}/ :

## VISUAL DESIGN DIRECTION

Comp 06 "Jira + Editorial Board" (`SPEC/mockups/ideas-boards-comp-06-jira-editorial-board.html`) is the selected authenticated-client UI/UX look and feel. It uses a Jira-inspired navy/blue horizontal shell and dense list pages with the open editorial Board composition. See `SPEC/20-feature-client-ui.md` (Visual Design Direction) for the authoritative layout, typography, palette, responsive, and implementation-boundary specification.