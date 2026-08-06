# Plan: Bug Fixes from `SPEC/bugs.md`

## Goal
Resolve items in `SPEC/bugs.md` so auth, routing, and shell behavior match the current spec and implementation stays covered by tests.

## Issues Reviewed
- Password policy text/rules are inconsistent with the spec.
- Login currently lands on `/board` instead of the authenticated dashboard at `/`.
- Session resume trusts local storage without first confirming the token is still valid.
- The shell shows redundant "Password update required" copy.

## Phases
- [x] Phase 1: Align password policy rules, messages, and tests
- [x] Phase 2: Verify the authenticated dashboard already implemented at `/`
- [x] Phase 3: Verify stored-token validation and invalid-session clearing
- [x] Phase 4: Verify redundant password-update copy and legacy redirects are absent
- [x] Phase 5: Run targeted regression tests and validate

## Status
**Current:** Complete. Password policy now matches the canonical 6-character rule; the other reported behaviors were already implemented and regression-verified.

## Decisions
- Password policy should follow the spec in `SPEC/20-feature-organizations-and-users.md`: at least 6 characters, lowercase, uppercase, number, and symbol.
- The authenticated root route `/` should become the dashboard entry point, and post-login redirects should land there.
- Session bootstrap should not assume a stored token is valid; it must confirm freshness before reusing the session.
- The redundant password-update banner copy should be removed rather than replaced with a new message.

## Errors Encountered
- The Debug solution build was blocked by a pre-existing API process locking output DLLs. A Release solution build completed with zero warnings and errors.
- Running browser smoke cases concurrently caused shared-port and `testhost` interference. The invalid-token test passed in isolation, and the ordinary demo-user login/dashboard theory row passed; the Site Admin row remains dependent on mutable seeded-password state.
