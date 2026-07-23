# Quickstart Validation: SargentNexus MVP

## Goal
Validate the core user-facing and admin-facing flows described by the Spec Kit port.

## Scenarios

### 1. Bootstrap and first login
1. Start the system with a valid environment-provided Site Admin credential.
2. Verify the seeded Site Admin can log in.
3. Verify the first successful login requires password change before other protected use.

### 2. Organization creation bootstrap
1. As Site Admin, create an organization.
2. Verify the organization is created with the default statuses.
3. Verify the organization is created with one default board.

### 3. User administration
1. As Org Admin, create users in the organization.
2. Verify duplicate email is rejected within the same organization.
3. Verify a duplicate email can exist in a different organization.
4. Verify the last Org Admin cannot remove their own admin access.

### 4. Login organization selection
1. Create the same email in two organizations.
2. Attempt login with that email.
3. Verify the flow requires organization selection before final authentication.

### 5. Board and status administration
1. Create a board with fewer than two swimlanes and verify rejection.
2. Reorder swimlanes and verify the order persists immediately.
3. Delete a status and verify it is soft-deleted rather than hard-removed from active references.

### 6. Idea collaboration
1. Create an idea with title and description.
2. Verify the default status uses the left-most swimlane when not specified.
3. Add tags with different casing and whitespace and verify normalization.
4. Mention a same-organization user by email in the idea body or comment.
5. Verify Read Only users can comment and upvote but cannot edit idea content.
6. Verify a User can move any idea on an eligible board.

### 7. Audit and notification events
1. Perform login, organization admin, and idea lifecycle actions.
2. Verify audit events are generated for the required actions.
3. Verify notification events are generated for mentions, comments, and status changes.
4. Verify outbound email is not required for MVP completion.
