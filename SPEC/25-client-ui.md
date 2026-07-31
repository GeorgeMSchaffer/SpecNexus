## NAVIGATION

Primary navigation is a horizontal menu under the main header: Home, Workflow, Ideas. The admin area (renamed "Settings") is reached via a gear icon in the header, not the menu. See SPEC/20-feature-client-ui-revisions.md for layout and Settings details.

- /Home: Displays a list of various boards the user has access to as well a dashboard
- /workflow: List of boards the user can access; clicking a board opens its swimlane view
- /ideas: List of ideas created by or assigned to the current user, with inline edit form
- /settings: Settings landing page (formerly Admin) with My Profile and role-scoped admin links.
    /settings/organizations :  Create and Manage organizations (list-first, form on create/edit)
    /settings/organizations/{orgId}/users: Create and Manage Users for an org
    /settings/organizations/{orgId}/statuses:  Create and manage statuses for the org

- /boards/{orgId}/board/{boardId}/ : Board swimlane view