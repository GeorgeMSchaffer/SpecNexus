# Formal Persona Review Report

Date: 2026-07-23
Scope: `SPEC` source docs and `SPECKIT` execution artifacts
Personas used: Code Reviewer, UI/UX Reviewer, Implementation Reviewer

## Findings

### High

1. Auth identity model conflicts across canonical and SPECKIT umbrella artifacts.
- Impact: implementation will branch incorrectly for login and user creation rules.
- Evidence:
  - `SPEC/20-feature-auth.md` and `SPEC/20-feature-organizations-and-users.md` define global email uniqueness.
  - `SPEC/SPECKIT/specs/001-sargentnexus-mvp/spec.md` still allows duplicate emails across organizations.
  - `SPEC/SPECKIT/specs/001-sargentnexus-mvp/spec.md` still requires organization selection during login.
- Recommended fix: update `001` umbrella spec to global unique email and remove organization-selection flow.

2. Umbrella API contract still documents optional `organizationId` login disambiguation.
- Impact: API and client may implement a deprecated branch not allowed by current requirements.
- Evidence:
  - `SPEC/SPECKIT/specs/001-sargentnexus-mvp/contracts/api.md` includes optional `organizationId` and alternate `200` organization-selection response.
  - Current canonical contract in `SPEC/30-Contracts.md` describes globally unique email login without org-selection response.
- Recommended fix: rewrite `001` contracts section for `POST /api/v1/auth/login` to single-path authentication behavior.

3. Organizations/users split feature still uses old uniqueness rule.
- Impact: persistence constraints and acceptance tests can be authored incorrectly.
- Evidence:
  - `SPEC/SPECKIT/specs/003-organizations-and-users/spec.md` says email is unique within an organization.
  - `SPEC/SPECKIT/specs/003-organizations-and-users/quickstart.md` expects duplicates across organizations to succeed.
  - Canonical specs require globally unique email.
- Recommended fix: align split feature 003 spec, quickstart, and related tasks/data model notes to global uniqueness.

### Medium

4. Implementation plan and roadmap still contain stale uniqueness and login-disambiguation wording.
- Impact: sequencing and work estimation drift in Phase 2 and Phase 3 implementation.
- Evidence:
  - `SPEC/50-technical-implementation-plan.md` Phase 3 still says "Enforce organization-scoped email uniqueness".
  - `SPEC/50-technical-implementation-plan.md` architecture notes still mention "login disambiguation".
  - `SPEC/70-delivery-backlog.md` still lists "enforce email uniqueness within an organization".
  - `SPEC/80-workstream-roadmap.md` mentions "normalized email uniqueness" without clarifying global scope.
- Recommended fix: normalize all planning/backlog wording to global uniqueness and remove disambiguation references.

5. Per-feature 003 OpenAPI is weaker than shared OpenAPI schema constraints.
- Impact: contract-first implementation may miss critical field limits in per-feature execution.
- Evidence:
  - `SPEC/SPECKIT/openapi/components/schemas/organizations-users.yaml` includes max length constraints.
  - `SPEC/SPECKIT/specs/003-organizations-and-users/contracts/openapi.yaml` omits those max lengths for organization and user fields.
- Recommended fix: copy or reference shared constrained schemas from `SPECKIT/openapi/components/schemas/organizations-users.yaml`.

6. Audit event actor nullability is inconsistent between plan persistence and split data model.
- Impact: uncertainty for system-generated events and schema nullability.
- Evidence:
  - `SPEC/50-technical-implementation-plan.md` persistence model allows nullable `actor_user_id`.
  - `SPEC/SPECKIT/specs/006-notifications-and-audit/data-model.md` lists `actorUserId` without nullability note.
- Recommended fix: explicitly define whether system events are allowed and align both artifacts.

### Low

7. UI/UX review process is now documented but not yet tied to a recurring cadence artifact.
- Impact: review execution may be inconsistent sprint-to-sprint.
- Evidence:
  - Reviewer and checklist assets exist in `SPECKIT/reviewers` and `SPECKIT/checklists`.
  - No standing "review run log" template currently present.
- Recommended fix: add a small reusable review-run template under `SPECKIT/reviews/templates`.

## Persona Outcome Summary
- Code Reviewer: 4 findings (3 high, 1 medium)
- Implementation Reviewer: 5 findings (3 high, 2 medium)
- UI/UX Reviewer: 1 low finding

## Priority Remediation Order
1. Align email uniqueness and login flow across `001` and `003` SPECKIT artifacts.
2. Update umbrella `001` contracts to remove organization-selection behavior.
3. Normalize implementation/backlog/roadmap wording to global uniqueness.
4. Tighten per-feature 003 OpenAPI constraints to match shared schema constraints.
5. Resolve audit-event actor nullability decision in one canonical place.