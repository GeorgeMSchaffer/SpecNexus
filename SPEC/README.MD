# SargentNexus Specifications

This directory contains the canonical product and delivery specifications for active MVP implementation.

## Canonical Spec Set

- `Specs Overview.md` (single-document AI entrypoint that aggregates canonical behavior)
- `10-requirements.md`
- `20-feature-*.md`
- `30-Contracts.md`
- `40-test-strategy.md`
- `50-technical-implementation-plan.md`
- `70-delivery-backlog.md`
- `80-workstream-roadmap.md`
- `90-definition-of-done.md`

## Working Rule

- Update these `SPEC/*.md` files first when implementation decisions change behavior.
- Sync derivative artifacts under `SPEC/SPECKIT` after canonical updates.

## AI Usage

- Start implementation work by reading `Specs Overview.md` first.
- For endpoint and payload details, cross-check `30-Contracts.md` and `SPECKIT/openapi/openapi.yaml`.
- For new client layout/interaction specs, start from `ui-ux-spec-template.md`.
