# Spec Kit Handoff Guide

## Purpose
Use this guide to migrate the manual `SPECKIT` port into a real repository initialized by the `specify` CLI.

## Current Repository Policy (2026-07-24)
- `SPEC/*.md` is the canonical source for active MVP decisions.
- `SPECKIT` artifacts are derived and synced from canonical docs.
- Split features `002-007` are the active organization model for this port.
- `001-sargentnexus-mvp` is retained as an archived umbrella reference.
- Per-feature `specs/*/contracts/openapi.yaml` files are the editable OpenAPI source in this port.
- `openapi/openapi.yaml` is a derived merged tooling surface.
- `007-kubernetes-containerization` is future/non-MVP exploratory scope.

## Recommended Migration Steps
1. Run `specify init` in the target repository.
2. Copy `.specify/memory/constitution.md` from this directory into the initialized repo.
3. Keep split features as the active baseline and treat the umbrella feature as historical reference unless the target team intentionally consolidates.
4. For each feature to keep, create the matching Spec Kit feature directory and copy over:
   - `spec.md`
   - `plan.md`
   - `research.md`
   - `data-model.md`
   - `contracts/`
   - `quickstart.md`
   - `tasks.md`
5. Compare the copied files against the active Spec Kit templates or presets in the new repo.
6. Regenerate or refine tasks using the real `/speckit.tasks` flow if desired.
7. Keep per-feature `contracts/openapi.yaml` as editable source and generate or sync merged `openapi/openapi.yaml` as needed for tooling.

## Suggested Feature Mapping
- `002-authentication-and-access`
- `003-organizations-and-users`
- `004-boards-and-statuses`
- `005-ideas-and-engagement`
- `006-notifications-and-audit`

## Handoff Checklist
- constitution copied
- feature folders selected
- contract strategy selected: per-feature editable source with merged derived surface
- unresolved validation questions reviewed from `SPEC/60-spec-q-and-a-backlog.md`
- project-specific presets or overrides reviewed before regeneration
