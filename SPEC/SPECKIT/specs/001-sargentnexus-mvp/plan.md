# Implementation Plan: SargentNexus MVP

## Technical Context
- API: ASP.NET Core Web API
- Client: Blazor with Fluent UI
- Persistence: SQL Server 2022 with Entity Framework Core
- Architecture: layered solution with API, Application, Domain, Infrastructure, and Client projects

## Objectives
- Deliver the SargentNexus MVP with tenant-scoped collaboration features.
- Keep notification delivery deferred while still generating notification events.
- Keep contracts, audit behavior, and tests aligned with the source specs.

## Architecture Strategy
- Centralize organization scope and authorization rules in the Application layer.
- Use SQL Server as the system of record for tenant-owned entities, audit events, and notification events.
- Use `/api/v1` resource routes with problem-details-style errors.
- Apply field and shape validation at the API boundary and business-rule validation in Application and Domain layers.
- Use last-write-wins updates in MVP.

## Implementation Phases

### Phase 1: Foundation
- set up solution structure and references
- add DbContext, migrations, base entities, and audit storage approach
- implement `/api/v1` routing conventions, problem-details errors, and request validation plumbing
- establish OpenAPI generation strategy

### Phase 2: Authentication and Global Administration
- implement login with optional organization resolution
- implement password hashing, complexity enforcement, inactive-account denial, and 15-minute lockout behavior
- seed the global Site Admin from environment configuration
- implement first-login password change
- add audit events for login outcomes and password changes

### Phase 3: Organizations and Users
- implement organization create, edit, list, detail, and archive flows
- bootstrap new organizations with default statuses and one default board
- implement user CRUD and lifecycle state management
- enforce org-scoped email uniqueness and last-Org-Admin protections
- audit admin changes

### Phase 4: Boards and Statuses
- implement org-scoped statuses with soft-delete behavior
- implement boards with swimlane subsets and minimum-two-swimlane validation
- persist reorder actions immediately

### Phase 5: Ideas and Engagement
- implement idea create, edit, list, detail, and status update flows
- implement tag normalization, autocomplete, and merge-on-concurrency handling
- implement email-based mentions for ideas and comments
- implement comments and upvotes with role-appropriate behavior
- emit audit events for idea lifecycle actions

### Phase 6: Notification Events
- emit notification events for mentions, comments, and status changes
- persist canonical idea link context for future email delivery
- keep outbound email delivery out of MVP

### Phase 7: Client Experience
- build tenant-aware login UX
- build Admin experiences for organizations and users
- build board, idea, comment, tag, mention, and upvote workflows
- reflect role boundaries in UI affordances

### Phase 8: Hardening
- align OpenAPI with contracts
- complete unit, integration, contract, and end-to-end test coverage
- verify deferred scope remains deferred

## Deliverables
- complete feature implementation across API, Application, Domain, Infrastructure, and Client
- aligned contracts and OpenAPI
- executable test coverage for the documented acceptance criteria
