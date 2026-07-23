# Data Model: Authentication and Access

## User Fields Relevant to Auth
- `userId`
- `organizationId` nullable for Site Admin
- `email`
- `normalizedEmail`
- `passwordHash`
- `role`
- `status`
- `mustChangePassword`

## Auth Rules
- inactive users cannot authenticate
- email is globally unique across the system
- password changes must satisfy password policy
- login outcomes are audited
