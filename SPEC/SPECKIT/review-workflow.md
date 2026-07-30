# Review Workflow

## Purpose
Define when to use the reviewer personas and checklists during the SargentNexus spec and implementation lifecycle.

## Source-of-Truth Rule
- Review canonical `SPEC/*.md` artifacts first for implementation intent and behavior.
- Use `SPECKIT` artifacts as derived cross-checks and workflow helpers.
- If drift is found, resolve canonical `SPEC` docs first, then sync `SPECKIT`.

## Mandatory Drift Gate
- Every review stage must include a canonical-vs-derived drift check.
- Derived-only behavior changes are blocking findings until canonical `SPEC` docs are updated and synchronized.
- Sign-off is blocked when any impacted `SPECKIT` artifact is stale against canonical `SPEC` sources.

## Review Stages

### 1. Spec Review
Use when feature specs, requirements, or contracts are drafted or updated.

Run:
- `Code Reviewer`
- `UI/UX Reviewer`
- `Implementation Reviewer`

Focus:
- canonical-first correctness against `SPEC/*.md`
- completeness of product rules
- implementation ambiguity
- workflow clarity
- validation and state handling
- derived artifact freshness and sync scope

### 2. Plan Review
Use when `30-Contracts.md`, `50-technical-implementation-plan.md`, `70-delivery-backlog.md`, or related SPECKIT planning artifacts change.

Run:
- `Code Reviewer`
- `Implementation Reviewer`
- optionally `UI/UX Reviewer` when client flows or screen responsibilities are affected

Focus:
- architecture fit
- work sequencing
- service boundaries
- persistence integrity
- testability
- stale-derived-file detection and remediation order

### 3. UI Review
Use when mockups, client workflows, or page plans change.

Run:
- `UI/UX Reviewer`
- optionally `Code Reviewer` for client-side rule leakage
- optionally `Implementation Reviewer` when navigation or workflow changes affect milestone slicing

Focus:
- role clarity
- empty states
- validation behavior
- visual and interaction consistency
- canonical-to-mockup behavior traceability

### 4. Implementation Review
Use during or after code changes.

Run:
- `Code Reviewer`
- `UI/UX Reviewer` for client-side work
- `Implementation Reviewer`

Focus:
- drift from specs or contracts
- maintainability and practical design quality
- missing tests
- usability regressions
- canonical and derived sync completeness

## Suggested Sequence
1. Review canonical feature specs under `SPEC/`.
2. Review contracts and implementation plan.
3. Review SPECKIT artifacts for derivative consistency and review readiness.
4. Review UI mockups or client behavior.
5. Review implemented changes before sign-off.
6. Fail sign-off if any derived artifact is stale or introduces behavior absent from canonical `SPEC` docs.

## Spec-Kit Style Invocation
Use prompt files in `.specify/prompts` and command wrappers in `.specify/commands` to run consistent reviews:
- `.specify/commands/review-code.md`
- `.specify/commands/review-ui-ux.md`
- `.specify/commands/review-implementation.md`
