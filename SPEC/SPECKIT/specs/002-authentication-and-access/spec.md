# Feature Specification: Authentication and Access

## Summary
Implement organization-scoped authentication with globally unique email credentials, seeded global Site Admin bootstrap, password rules, inactive-account denial, and account lockout behavior.

## Requirements
- Users authenticate with email and password.
- User email is globally unique across the system.
- Passwords follow the defined complexity policy.
- Five failed login attempts within 15 minutes cause a 15-minute lockout.
- Inactive accounts cannot log in.
- The global Site Admin is seeded from an environment-provided initial credential and must change it on first login.
- Admin-issued temporary password reset is a later P1 capability and uses one-time display temporary passwords that expire after 24 hours and force password change on first use.
- Authentication outcomes and password actions are audited.
