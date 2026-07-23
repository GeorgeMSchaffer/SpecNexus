# Requirements: SargentNexus

## Overview
SargentNexus is a collaboration and project management tool for submitting, tracking, and improving process ideas.

## Global Rules
- All data is scoped to an organization.
- The Site Admin account is a global platform account and is not owned by any organization.
- Only Site Admin can create organizations.
- Org Admin can edit only their own organization.
- Organizations may be archived but are not hard-deleted.
- Statuses are organization-scoped.
- Tags are organization-scoped.
- Read Only users can comment and upvote, but cannot edit ideas or board configuration.

## Roles and Permissions

| Permission | Site Admin | Org Admin | User | Read Only |
|---|:---:|:---:|:---:|:---:|
| Create organizations | ✓ | | | |
| Edit own organization | ✓ | ✓ | | |
| Manage users (all orgs) | ✓ | | | |
| Manage users (own org) | ✓ | ✓ | | |
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
- **P2**: Remember this device