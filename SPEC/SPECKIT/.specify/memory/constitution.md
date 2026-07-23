# SargentNexus Constitution

## Article I: Layered Business Logic
- Business rules must live in the Domain and Application layers, never in controllers or UI components.
- The dependency direction remains API -> Application -> Domain, with Infrastructure implementing Application or Domain abstractions.
- The solution uses the established projects: `SargentNexus.API`, `SargentNexus.Application`, `SargentNexus.Domain`, `SargentNexus.Infrastructure`, and `SargentNexus.Client`.

## Article II: Organization Scope and Authorization
- All tenant-owned business data is organization-scoped.
- Site Admin is a global platform account and is not owned by any organization.
- Authorization behavior must match the written role boundaries for Site Admin, Org Admin, User, and Read Only.
- Org boundary enforcement must be centralized and cannot rely only on UI behavior.

## Article III: Contract and Validation Discipline
- HTTP APIs are versioned under `/api/v1` and use plural resource routes.
- All non-2xx responses use a problem-details-style error contract.
- API boundaries validate shape, required fields, and basic field constraints.
- Application and Domain layers enforce business rules, authorization rules, and invariants.

## Article IV: Security and Secret Handling
- No hardcoded credentials or secrets are allowed.
- The seeded Site Admin uses an environment-provided initial credential and must change it on first login.
- Password policy, login lockout, and inactive-account behavior must match the written authentication requirements.

## Article V: Spec and Test Traceability
- Feature behavior must match the relevant feature specs.
- Contract documents and OpenAPI must be updated whenever endpoint behavior or payloads change.
- Acceptance criteria are covered by targeted tests.
- Regression risk is covered by focused unit, integration, contract, or end-to-end validation.
