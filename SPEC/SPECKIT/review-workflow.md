# Review Workflow

## Purpose
Define when to use the reviewer personas and checklists during the SargentNexus spec and implementation lifecycle.

## Review Stages

### 1. Spec Review
Use when feature specs, requirements, or contracts are drafted or updated.

Run:
- `Code Reviewer`
- `UI/UX Reviewer`
- `Implementation Reviewer`

Focus:
- completeness of product rules
- implementation ambiguity
- workflow clarity
- validation and state handling

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

## Suggested Sequence
1. Review feature specs.
2. Review contracts and implementation plan.
3. Review UI mockups or client behavior.
4. Review implemented changes before sign-off.

## Spec-Kit Style Invocation
Use prompt files in `.specify/prompts` and command wrappers in `.specify/commands` to run consistent reviews:
- `.specify/commands/review-code.md`
- `.specify/commands/review-ui-ux.md`
- `.specify/commands/review-implementation.md`
