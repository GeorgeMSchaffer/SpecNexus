# Feature: Organizations and Users

## Outcome
Administrators can manage organizations and users with clear role boundaries within a dedicated Admin section of the application.

## Organization Rules
1. Organizations are the top-level ownership boundary for all business data.
2. Only Site Admin can create organizations.
3. Site Admin and Org Admin can edit organization details for organizations they administer.
4. Organizations can be archived but cannot be hard-deleted.
5. A newly created organization starts with the default statuses and one default board.
6. Organization administration screens must provide guided empty states with a primary action and short explanatory text when no relevant records exist.

## Organization Fields
- Company Name (max 200 characters)
- Address (max 200 characters)
- City (max 100 characters)
- State (max 50 characters)
- Zip (max 20 characters)
- Phone (max 25 characters)
- Primary Contact First Name (max 100 characters)
- Primary Contact Last Name (max 100 characters)

All organization text fields are trimmed before validation and persistence.

## User Rules
1. Site Admin can manage users across all organizations.
2. Org Admin can manage users only within their own organization.
3. Each non-Site Admin user belongs to exactly one organization.
4. Each user must have one role.
5. Email is used as the user's mention identity in collaboration features.
6. User email must be globally unique across the system.
7. Site Admin is a global account and does not belong to an organization.
8. An Org Admin cannot remove their own Org Admin role or deactivate themselves if they are the last Org Admin in that organization, only the Site Admin can deactivate the organization.
9. Administrators handle password reset by issuing temporary passwords in P1.
10. User accounts support `Active` and `Inactive` states only in MVP.
11. Organization changes, user changes, role changes, and account status changes must be audited.

## User Fields
- First Name (max 100 characters)
- Last Name (max 100 characters)
- Email
- Password
- Role
- Organization
- Status (`Active` or `Inactive`)

User profile text fields are trimmed before validation and persistence.

## Credential Rules
- Passwords must satisfy the authentication complexity policy.

## Roles
- **Site Admin**: global administrator
- **Org Admin**: organization administrator
- **User**: standard contributor
- **Read Only**: limited participant

## Acceptance Criteria
- [ ] Site Admin can create organizations
- [ ] Org Admin cannot create organizations
- [ ] Organizations can be archived without being hard-deleted
- [ ] Archived organizations are hidden from admin lists by default unless explicitly filtered for archived items
- [ ] New organizations are provisioned with default statuses and one default board
- [ ] Admin screens provide guided empty states with a primary action and short explanatory text
- [ ] Site Admin can manage users across organizations
- [ ] Org Admin can manage users only in their organization
- [ ] Site Admin is not required to belong to an organization
- [ ] User email is available for collaboration features that resolve mentions
- [ ] User email is globally unique across the system
- [ ] Organization and user text fields are trimmed and validated against their maximum lengths
- [ ] The last Org Admin in an organization cannot remove their own admin access or deactivate themselves
- [ ] Inactive users cannot authenticate
- [ ] Organization and user administration actions generate audit events
- [ ] Users are assigned exactly one organization and one role