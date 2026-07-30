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

## Canonical Precedence Contract

- Canonical behavior authority lives only in `SPEC/*.md`.
- `SPEC/SPECKIT` is a derived review and delivery package and must not introduce net-new behavior.
- If `SPEC` and `SPECKIT` disagree, resolve `SPEC` first and then regenerate or sync `SPECKIT`.
- Do not approve PRs that update behavior in `SPECKIT` without the corresponding canonical `SPEC` update.

## Deterministic Sync Workflow

1. Edit canonical behavior in `SPEC/*.md`.
2. Validate internal canonical consistency across requirements, feature, contract, and test docs.
3. Sync affected artifacts under `SPEC/SPECKIT` (feature specs, plans, tasks, contracts, and checklists as needed).
4. Run a drift check before merge and block merge if derived artifacts are stale.

Reference: `SPEC/SPECKIT/sync-workflow.md` defines canonical-to-derived mapping and required sync gates.

## AI Usage

- Start implementation work by reading `Specs Overview.md` first.
- For endpoint and payload details, cross-check `30-Contracts.md` and `SPECKIT/openapi/openapi.yaml`.
- For new client layout/interaction specs, start from `ui-ux-spec-template.md`.
