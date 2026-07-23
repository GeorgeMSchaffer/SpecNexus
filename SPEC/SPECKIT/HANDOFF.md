# Spec Kit Handoff Guide

## Purpose
Use this guide to migrate the manual `SPECKIT` port into a real repository initialized by the `specify` CLI.

## Recommended Migration Steps
1. Run `specify init` in the target repository.
2. Copy `.specify/memory/constitution.md` from this directory into the initialized repo.
3. Decide whether to keep the umbrella feature, the split features, or both.
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
7. Align OpenAPI generation with either the per-feature `contracts/openapi.yaml` files or the merged `openapi/openapi.yaml` root file.

## Suggested Feature Mapping
- `002-authentication-and-access`
- `003-organizations-and-users`
- `004-boards-and-statuses`
- `005-ideas-and-engagement`
- `006-notifications-and-audit`

## Handoff Checklist
- constitution copied
- feature folders selected
- contract strategy selected: per-feature or merged openapi
- unresolved validation questions reviewed from `SPEC/60-spec-q-and-a-backlog.md`
- project-specific presets or overrides reviewed before regeneration
