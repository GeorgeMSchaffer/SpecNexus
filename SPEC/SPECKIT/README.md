# SargentNexus Spec Kit Port

This directory is a manual port of the current `SPEC` documents into a Spec Kit style structure.

## Governance
- `SPEC/*.md` remains the source of truth for active MVP implementation.
- This `SPECKIT` directory is a derived reference and review-tooling package.
- When behavior changes, update canonical `SPEC` docs first, then sync `SPECKIT`.

## Source Mapping
- Product and feature requirements came from the `SPEC/10-requirements.md` and `SPEC/20-feature-*.md` files.
- Technical planning came from `SPEC/50-technical-implementation-plan.md`.
- Contract details came from `SPEC/30-Contracts.md`.
- Delivery tasks came from `SPEC/70-delivery-backlog.md`.
- Quality gates came from `SPEC/40-test-strategy.md` and `SPEC/90-definition-of-done.md`.

## Structure
- `.specify/memory/constitution.md`: project principles
- `.specify/extensions.yml`: minimal project-local Spec Kit scaffolding
- `.specify/templates/overrides/README.md`: location for local template overrides
- `HANDOFF.md`: guide for migrating this manual port into a real `specify`-initialized repository
- `openapi/`: composable shared OpenAPI layout with path and schema fragments
- `specs/001-sargentnexus-mvp/`: archived umbrella reference for the full MVP (read-only)
- `specs/002-authentication-and-access/`: split feature for authentication and access
- `specs/003-organizations-and-users/`: split feature for tenant administration
- `specs/004-boards-and-statuses/`: split feature for workflow configuration
- `specs/005-ideas-and-engagement/`: split feature for collaboration workflows
- `specs/006-notifications-and-audit/`: split feature for notification events and audit coverage
- `specs/007-kubernetes-containerization/`: future/non-MVP exploratory feature for container and Kubernetes planning

## Notes
- This is structured to resemble a Spec Kit initialized project, but it was generated manually from the existing specs rather than by running `specify init` and the slash commands.
- The `001-sargentnexus-mvp` directory is kept only as a historical umbrella reference and is not the active editing target.
- The `002` through `007` directories are the active split-feature organization for this port.
- Each split feature includes `research.md`, `data-model.md`, `quickstart.md`, `spec.md`, `plan.md`, and `tasks.md`.
- API-facing split features use `contracts/openapi.yaml` as the editable source.
- The `openapi/` directory is a derived merged surface for tooling and should not be treated as the primary hand-edited source.
- The remaining unresolved questions stay tracked in `SPEC/60-spec-q-and-a-backlog.md`.


