# Feature Specification: SargentNexus MVP

> Archived umbrella reference. Active edits should be made in split features `002` through `006`.

**Feature Branch**: `001-sargentnexus-mvp`

## Summary
SargentNexus is a collaboration and project management product for submitting, tracking, and improving process ideas inside organizations. The MVP supports authentication with globally unique email credentials, administrative setup, configurable workflow boards, idea collaboration, and audit-ready event generation.

## Problem
Organizations need a structured way to capture process ideas, discuss them, move them through a lightweight workflow, and preserve role boundaries without requiring a full enterprise project-management platform.

## Users
- Site Admin: platform administrator across all organizations
- Org Admin: administrator within one organization
- User: standard contributor within one organization
- Read Only: collaborator who can comment and upvote but cannot edit idea content

## User Stories
1. As a Site Admin, I want to create and bootstrap organizations so new tenants can begin using the product immediately.
2. As an Org Admin, I want to manage users, statuses, and boards in my organization without affecting other organizations.
3. As a User, I want to create, edit, discuss, tag, mention, and move ideas through board workflows.
4. As a Read Only user, I want to participate through comments and upvotes without editing idea content.
5. As a product owner, I want audit events for sensitive and collaboration-critical actions.

## Functional Requirements

### Authentication and Access
- Users authenticate with email and password.
- User accounts are organization-scoped, and email is globally unique across the system.
- Passwords follow the configured complexity policy.
- Five failed login attempts within 15 minutes trigger a 15-minute lockout.
- Inactive users cannot authenticate.
- The seeded Site Admin is a global account created from an environment-provided initial credential and must change that credential on first login.
- In Development only, startup seed also creates demo Org Admin, User, and Read Only accounts for each demo organization, all initialized with temporary password `abc123!` and forced password change on first successful login.
- Admin-issued temporary password reset is a P1 capability.
- Self-service password reset by a private 24-hour email link is deferred until post-MVP; anonymous email-only password replacement is not allowed.
- OAuth/OIDC implementation is deferred to post-MVP Phase 2 (Microsoft Entra ID first).
- SAML implementation is deferred to a post-OAuth phase.

### Organizations and Users
- Only Site Admin can create organizations.
- Site Admin and Org Admin can edit organization details in scope.
- Organizations can be archived but not hard-deleted.
- Each new organization is provisioned with default statuses and one default board.
- An invite code is auto-generated when an organization is created; displayed in list and detail views.
- Admins can regenerate an organization's invite code; archived org codes are invalid.
- Users can self-register using a valid invite code, which sets their organization.
- In Development only, startup seed creates 3 demo organizations with realistic profile data.
- Non-Site Admin users belong to exactly one organization and have exactly one role.
- User email is globally unique across the system.
- Org Admins cannot remove their own admin role or deactivate themselves if they are the last Org Admin in the organization.
- User lifecycle states are limited to `Active` and `Inactive` in MVP.

### Boards and Statuses
- Statuses are organization-scoped.
- Default statuses are `New / Pending`, `In Review`, `In Progress`, `Client Review`, and `Complete`.
- Status deletion is soft-delete only.
- Boards use swimlanes mapped to statuses.
- A board must have at least two swimlanes.
- Board swimlane order is saved immediately after drag-and-drop reorder.
- In Development only, each seeded demo organization includes one example board populated with ideas across all default swimlanes.
- Site Admin and Org Admin can manage boards and statuses.

### Ideas and Engagement
- Ideas require a title and description.
- Idea titles are limited to 150 characters.
- Idea descriptions are limited to 4000 characters.
- Ideas require a priority value: `Low`, `Medium`, `High`, or `Critical`.
- Ideas may optionally include a due date.
- An idea status is derived from the swimlane it belongs to and defaults to the left-most swimlane if omitted on create.
- Board cards are compact and show only title, priority, assigned-to, and upvote state.
- Clicking an idea title opens a detail overlay so editing remains in context without page navigation.
- Completed ideas remain editable and collaborative.
- Users who can edit ideas can create tags.
- Tag autocomplete starts after two entered characters.
- Tags are trimmed, case-insensitive, unique within an organization, limited to 100 characters, and merged on concurrent duplicate creation.
- Mentions resolve users by email within the same organization for both ideas and comments.
- All authenticated users, including Read Only, can comment and upvote.
- Comment authors can edit and delete their own comments.
- Site Admin and Org Admin can delete comments in their authorized scope.
- Upvoting is a toggle with one active upvote per user per idea.
- Only the user who cast an upvote can remove it.
- If board configuration allows it, Users can move any idea on that board.
- Development seeded ideas include description-based sample spec content and example comments for collaboration walkthroughs.

### Notifications and Audit
- Notification events are generated for idea mentions, comment mentions, comments on ideas, and idea status changes.
- Guaranteed outbound email delivery is deferred outside MVP.
- Audit events are generated for auth actions, admin actions, and idea lifecycle actions.

## Non-Functional Requirements
- Business rules remain in Application and Domain layers.
- No hardcoded credentials or secrets.
- API contracts remain aligned with the written contracts document.
- Acceptance criteria are covered by tests.

## Success Criteria
- Role boundaries and organization scoping hold for all user-visible workflows.
- New organizations are immediately usable through seeded statuses and a default board.
- Collaboration workflows support ideas, tags, comments, mentions, and upvotes as specified.
- Deferred capabilities such as OAuth, reporting, and guaranteed email delivery remain out of MVP.
- OAuth and SAML are tracked as planned post-MVP capabilities and are not MVP release blockers.
