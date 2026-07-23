# Code Review Checklist

## Spec Integrity
- [ ] Requirements are concrete enough to implement without guessing.
- [ ] Acceptance criteria describe observable behavior rather than intent only.
- [ ] Lifecycle rules are defined for create, update, archive, delete, and recovery paths where relevant.
- [ ] Permission rules are explicit for every role involved in the feature.
- [ ] Tenant-scope behavior is unambiguous.

## Contracts and Validation
- [ ] Request shapes, response shapes, and error behavior are defined.
- [ ] Field constraints are specified for all required fields.
- [ ] Sensitive flows define expiry, one-time-use, retry, and failure behavior.
- [ ] Public API behavior matches the written contracts.
- [ ] Validation ownership between API and Application or Domain layers is clear.

## Architecture and Persistence
- [ ] Business rules remain out of controllers and UI components.
- [ ] Persistence rules support the stated lifecycle behavior safely.
- [ ] Data integrity constraints are identified where they matter.
- [ ] Soft-delete, archive, and historical display behavior are compatible.
- [ ] Event generation and audit storage are tied to the correct workflows.

## Implementation Practicality
- [ ] The simplest viable implementation path is apparent.
- [ ] No speculative abstractions are required to satisfy the current spec.
- [ ] Service boundaries are clear enough to distribute work across layers.
- [ ] Deferred scope is clearly separated from MVP scope.

## Testability
- [ ] Acceptance criteria are traceable to test cases.
- [ ] High-risk rules have unit or integration validation paths.
- [ ] Contract changes imply contract test updates.
- [ ] Regression-prone workflows are identifiable.
