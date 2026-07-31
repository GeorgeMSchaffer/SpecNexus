# Requirements: SargentNexus

## Overview
SargentNexus is a collaboration and project management tool for submitting, tracking, and improving process ideas.

## Global Rules
- All data is scoped to an organization.
- The Site Admin account is a global platform account and is not owned by any organization.
- Only Site Admin can create organizations.
- Org Admin can edit only their own organization.
- All product screens use a unified application shell with a persistent top header, organization identity, role-aware navigation, and breadcrumb navigation.
- Organizations may be archived but are not hard-deleted.
- Organizations can upload one logo at a time from the organization edit form; uploading a new logo replaces the previous logo.
- Statuses are organization-scoped.
- Tags are organization-scoped.
- Read Only users can comment and upvote, but cannot edit ideas or board configuration.
- In Development, startup seed creates a demo environment with 3 organizations, each containing Org Admin, User, and Read Only accounts initialized with temporary password `abc123!` and forced password change on first login.
- In Development, each seeded demo organization includes one example board with ideas across every default swimlane, plus example comments and description-based spec content.
- Ideas require a priority (`Low`, `Medium`, `High`, `Critical`) and may optionally include a due date.
- Board cards are compact and display title, priority, assigned-to, and upvote state.
- Clicking an idea title opens an in-context detail overlay for editing rather than a full-page navigation.
- OAuth implementation is scheduled for post-MVP Phase 2, with SAML scheduled in a subsequent post-OAuth phase.

## UI Shell Rules
- Every screen includes a persistent header with primary blue background, logo at top-left, and global actions.
- The header reserves a `150px` brand zone on the left for logo and product identity.
- Every screen includes role-aware primary navigation in a consistent location.
- The header includes a logout icon action.
- Admin-authorized users see a gear icon action in the header that navigates to the Admin homepage.
- Breadcrumb navigation appears immediately below the header and reflects current location with upward navigation.
- Organization logos rendered in the header must be constrained to a maximum rendered height of `150px` and keep aspect ratio.

## Roles and Permissions

| Permission | Site Admin | Org Admin | User | Read Only |
|---|:---:|:---:|:---:|:---:|
| Create organizations | ✓ | | | |
| Edit own organization | ✓ | ✓ | | |
| Manage users (all orgs) | ✓ | | | |
| Manage users (own org) | ✓ | ✓ | | |
| Import users by CSV (authorized orgs) | ✓ | ✓ | | |
| Create/manage boards | ✓ | ✓ | | |
| Manage statuses | ✓ | ✓ | | |
| View boards and ideas | ✓ | ✓ | ✓ | ✓ |
| Create/edit ideas | ✓ | ✓ | ✓ | |
| Update idea status | ✓ | ✓ | ✓* | |
| Comment on ideas | ✓ | ✓ | ✓ | ✓ |
| Upvote ideas | ✓ | ✓ | ✓ | ✓ |
| Mention users | ✓ | ✓ | ✓ | |

*If permitted by board configuration, Users can update the status of any idea on that board.

## Priorities
- **P0**: Authentication, organization management, user/role management, boards, statuses, idea CRUD, comments, upvote
- **P1**: Password reset, email notifications
- **P2**: OAuth/OIDC (Microsoft Entra ID first)
- **P3**: SAML
- **P4**: Remember this device