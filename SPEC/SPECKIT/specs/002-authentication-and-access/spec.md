# Feature Specification: Authentication and Access

## Derived Sync Metadata
- Status: Derived
- Canonical Sources:
	- `SPEC/10-requirements.md`
	- `SPEC/20-feature-auth.md`
	- `SPEC/20-feature-user-login.md`
	- `SPEC/30-Contracts.md`
	- `SPEC/40-test-strategy.md`
- Last Canonical Sync Date: 2026-08-04

## Summary
Implement authentication with globally unique email credentials, seeded global Site Admin bootstrap, password rules, inactive-account denial, and account lockout behavior.

## Requirements
- Users authenticate with email and password.
- User accounts are organization-scoped, and email is globally unique across the system.
- Passwords follow the defined complexity policy.
- Five failed login attempts within 15 minutes cause a 15-minute lockout.
- Inactive accounts cannot log in.
- The global Site Admin is seeded from an environment-provided initial credential and must change it on first login.
- In Development only, startup seed creates demo Org Admin, User, and Read Only accounts in each demo organization using temporary password `abc123!`.
- Development seeded demo users must change password on first successful login.
- Admin-issued temporary password reset is an MVP/P1 capability and uses one-time display temporary passwords that expire after 24 hours and force password change on first use.
- Post-MVP self-service reset sends active local-password users a private, single-use bearer-token link that expires after 24 hours and is invalidated by a newer request.
- Post-MVP reset requests use a generic response, limit delivery to 3 per normalized email and 10 per source IP within 15 minutes, and do not reveal account eligibility.
- Post-MVP reset confirmation requires matching passwords that satisfy the existing complexity policy, consumes the token, revokes all sessions, and returns the user to Login.
- Invalid, expired, superseded, and used reset tokens share one invalid-link state, and reset secrets are excluded from persistence, logs, audit metadata, analytics, and responses.
- Authentication outcomes and password actions are audited.
- OAuth/OIDC and SAML implementation are out of MVP scope and scheduled for post-MVP phases.
