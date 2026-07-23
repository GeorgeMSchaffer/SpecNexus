# Feature: Authentication

## Outcome
Users can securely access the application using organization-scoped accounts.

## Scope
- In: login, seeded Site Admin account, admin-issued password reset
- Out: SSO, MFA, social login

## Requirements
1. Users must authenticate with email and password.
2. User accounts are organization-scoped, and user email is globally unique across the system.
4. Unauthorized users cannot access protected application features.
5. Passwords must satisfy a traditional complexity policy with minimum length plus uppercase, lowercase, numeric, and special character requirements.
6. Five failed login attempts within 15 minutes must trigger a 15-minute account lockout.
7. The seeded Site Admin account is a global platform account and does not belong to an organization.
8. A seed Site Admin account must be created on first run using an environment-provided initial credential.
9. The seed Site Admin must be forced to change that initial credential on first login.
10. Password reset is required in P1 and uses admin-issued temporary passwords.
11. Admin-issued temporary passwords are shown one time only, expire after 24 hours, and require password change on first use.
12. "Remember this device" is out of scope for MVP.
13. Inactive user accounts cannot authenticate.
14. Successful and failed authentication events, password changes, and password resets must be audited.

## Acceptance Criteria
- [ ] Valid credentials allow login
- [ ] Invalid credentials are rejected
- [ ] Five failed login attempts within 15 minutes trigger a 15-minute lockout
- [ ] Inactive users cannot log in
- [ ] Password changes are rejected if they do not satisfy the password complexity policy
- [ ] Seed Site Admin is created at first run
- [ ] Seed Site Admin must change the environment-provided initial credential on first login
- [ ] Admin-issued temporary password reset is implemented in P1
- [ ] Temporary passwords are one-time display, expire after 24 hours, and force password change on first use
- [ ] Authentication outcomes and password-related actions generate audit events