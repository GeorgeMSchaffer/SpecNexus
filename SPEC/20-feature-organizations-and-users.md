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
7. Organization edit screens include logo upload with in-form preview for board branding.
8. Each organization can have only one active logo at a time.
9. Uploading a new logo replaces the previous logo.
10. The rendered organization logo height is capped at `150px` while preserving aspect ratio.
11. An invite code is auto-generated when an organization is created. The invite code is displayed in both the organization list view and organization detail page.
12. Admins can regenerate an organization's invite code. Invite codes for archived organizations are invalid for self-registration.
13. Users can self-register for an account if they have a valid invite code. The invite code entered at registration determines which organization the user is associated with.
14. Authenticated admin surfaces use a header with a brand zone and logo in the top-left.
15. The header exposes a sign-out icon left of the username display, and a gear icon that navigates to the Settings area. The Settings area was previously called "Admin".
16. The primary navigation is a horizontal menu under the header: Home, Workflow, Ideas. Settings is reached via the gear icon only.
17. Breadcrumb navigation is shown directly below the header.
18. In Development, startup seed creates 3 demo organizations with realistic profile data for walkthrough and validation.

## Organization Fields
- Title (max 200 characters, required)
- Description (max 500 characters, optional)
- Invite Code (system-managed, auto-generated on creation, regenerable by admin)
- Company Name (max 200 characters)
- Address (max 200 characters)
- City (max 100 characters)
- State (max 50 characters)
- Zip (max 20 characters)
- Phone (max 25 characters)
- Primary Contact First Name (max 100 characters)
- Primary Contact Last Name (max 100 characters)
- Logo URL (system-managed)
- Logo Thumbnail URL (system-managed)
- Logo Height Px (system-managed, max rendered value `150`)

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
12. Development startup seed creates one Org Admin, one User, and one Read Only user in each seeded demo organization.
13. Site Admin and Org Admin can import users through CSV within the same organization scope as individual user creation.
14. CSV import creates organization-scoped users only and cannot create a Site Admin.

## User CSV Import
- The user administration screen provides `Download CSV template` and `Import CSV` actions.
- The downloadable template is UTF-8 CSV with the exact header row `firstName,lastName,email,role,status,initialPassword` and one example row.
- Imported CSV files must use the template header names and order.
- `firstName`, `lastName`, `email`, `role`, and `initialPassword` are required for every non-blank row.
- `status` is optional and defaults to `Active`; when supplied, it must be `Active` or `Inactive`.
- `role` must be `Org Admin`, `User`, or `Read Only`.
- Imported names, emails, roles, and statuses are trimmed before validation; email uniqueness remains global.
- `initialPassword` must satisfy the authentication complexity policy and is not trimmed or returned in import results.
- Blank rows are ignored. A file containing no data rows is rejected.
- The import accepts at most 1,000 data rows and a maximum file size of 5 MB.
- The entire file is validated before persistence. Any invalid row, duplicate email within the file, or email already in the system rejects the complete import without creating users.
- Validation failures identify the one-based CSV row number, field, and message so administrators can correct and retry the file.
- A successful import returns the number of users created and writes user-administration audit events without storing plaintext passwords or CSV file contents.
- The organization user list refreshes after a successful import and displays a summary of the created-user count.

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
- [ ] A new organization auto-generates an invite code on creation
- [ ] Invite code is displayed in the organization list and detail views
- [ ] Admins can regenerate an organization's invite code
- [ ] Invite codes for archived organizations cannot be used for self-registration
- [ ] Users can self-register using a valid invite code which sets their organization
- [ ] Site Admin can create organizations
- [ ] Org Admin cannot create organizations
- [ ] Organizations can be archived without being hard-deleted
- [ ] Archived organizations are hidden from admin lists by default unless explicitly filtered for archived items
- [ ] New organizations are provisioned with default statuses and one default board
- [ ] Admin screens provide guided empty states with a primary action and short explanatory text
- [ ] Organization edit form supports logo upload and displays a thumbnail preview after upload
- [ ] Uploading a new organization logo replaces any previously stored organization logo
- [ ] Board header displays the current organization logo with rendered height no greater than `150px`
- [ ] Site Admin can manage users across organizations
- [ ] Org Admin can manage users only in their organization
- [ ] Development startup seed creates 3 demo organizations
- [ ] Each demo organization includes exactly one seeded Org Admin, one seeded User, and one seeded Read Only user
- [ ] Site Admin is not required to belong to an organization
- [ ] User email is available for collaboration features that resolve mentions
- [ ] User email is globally unique across the system
- [ ] Organization and user text fields are trimmed and validated against their maximum lengths
- [ ] The last Org Admin in an organization cannot remove their own admin access or deactivate themselves
- [ ] Inactive users cannot authenticate
- [ ] Organization and user administration actions generate audit events
- [ ] Users are assigned exactly one organization and one role
- [ ] Site Admin can download the user CSV template and import users for any organization
- [ ] Org Admin can download the user CSV template and import users only for their own organization
- [ ] The CSV template contains the documented headers and an example row
- [ ] A valid CSV import creates all users in the selected organization and reports the created-user count
- [ ] CSV import defaults an omitted status to `Active` and rejects unsupported roles or statuses
- [ ] CSV import rejects files over 5 MB or 1,000 data rows
- [ ] Invalid rows and duplicate emails return row-specific errors and create no users from the file
- [ ] CSV import cannot create Site Admin users and does not expose or audit plaintext passwords or file contents