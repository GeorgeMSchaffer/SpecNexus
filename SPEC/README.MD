# SargentNexus Specifications

This directory contains the canonical product and delivery specifications for active MVP implementation.

## Canonical Spec Set

- `Specs Overview.md` (single-document AI entrypoint that aggregates canonical behavior)
- `10-requirements.md`
- `20-feature-*.md` Denotes an individual features for implementation
- `30-Contracts.md`
- `40-test-strategy.md`
- `50-technical-implementation-plan.md`
- `70-delivery-backlog.md`
- `80-workstream-roadmap.md`
- `90-definition-of-done.md`

## Working Rule

- Update these `SPEC/*.md` files first when implementation decisions change behavior.

## Deterministic Sync Workflow

1. Edit canonical behavior in `SPEC/*.md`.
2. Validate internal canonical consistency across requirements, feature, contract, and test docs.

## AI Usage

- Start implementation work by reading `implementation-agent-tracker.md` for complete and pending items.
- For endpoint and payload details, use `30-Contracts.md` as the canonical source.
- For new client layout/interaction specs, start from `ui-ux-spec-template.md`.
