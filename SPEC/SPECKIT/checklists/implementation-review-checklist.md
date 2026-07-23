# Implementation Review Checklist

## Source-of-Truth Alignment
- [ ] Each critical behavior has one authoritative source document.
- [ ] Split feature specs agree with umbrella MVP docs on auth and identity rules.
- [ ] Contracts and OpenAPI artifacts encode current rule decisions.
- [ ] Roadmap and backlog wording matches current business constraints.

## Sequencing and Dependencies
- [ ] High-risk foundation work is scheduled before dependent features.
- [ ] Cross-layer dependencies are explicit for each milestone.
- [ ] Tasks can be executed in thin vertical slices, not only horizontal layers.
- [ ] Data model and migration assumptions are ready before API task execution.

## Implementation Readiness
- [ ] Field limits and validation ownership are complete and testable.
- [ ] Permission boundaries are concrete enough for authorization middleware and application guards.
- [ ] Sensitive flows define lockout, expiry, and failure behavior.
- [ ] Event generation behavior is tied to observable acceptance criteria.

## Delivery Safety
- [ ] Deferred scope is clearly excluded from MVP execution tasks.
- [ ] Change-risk areas have explicit rollback or mitigation notes.
- [ ] Drift checks exist between contracts and implementation docs.
- [ ] Milestone exits are measurable with existing tests or diagnostics.

## Verification
- [ ] Every high-risk rule maps to at least one integration or contract test target.
- [ ] Acceptance criteria are traceable to specific verification artifacts.
- [ ] Test strategy updates are triggered when contract behavior changes.