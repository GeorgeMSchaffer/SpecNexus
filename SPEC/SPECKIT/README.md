# SargentNexus Spec Kit Derived Package

This directory is a derived package generated and maintained from canonical `SPEC` documents.

## Governance
- `SPEC/*.md` remains the source of truth for active MVP implementation.
- This `SPECKIT` directory is a derived reference and review-tooling package.
- When behavior changes, update canonical `SPEC` docs first, then sync `SPECKIT`.
- Derived artifacts in this directory must not introduce behavior not present in canonical `SPEC` docs.

## Editing Policy
- Behavior edits start in `SPEC/*.md`, never in `SPECKIT`.
- `SPECKIT` edits are limited to synchronization, review ergonomics, and packaging unless canonical docs are updated in the same change.
- If drift exists, fix canonical `SPEC` first, then update `SPECKIT`.
- Pull requests should fail review when derived `SPECKIT` files are stale against canonical sources.

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
- `sync-workflow.md`: canonical-to-derived synchronization workflow and mapping
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
- The `002` through `006` directories are the active split-feature organization for this port.
- The `007` directory is retained for future/non-MVP exploratory planning and is not part of active MVP implementation synchronization.
- Each split feature includes `research.md`, `data-model.md`, `quickstart.md`, `spec.md`, `plan.md`, and `tasks.md`.
- API-facing split features contain derived contract surfaces synchronized from canonical contract behavior in `SPEC/30-Contracts.md` and related canonical feature specs.
- The `openapi/` directory is a derived merged surface for tooling and should not be treated as the primary hand-edited source.
- The remaining unresolved questions stay tracked in `SPEC/60-spec-q-and-a-backlog.md`.


