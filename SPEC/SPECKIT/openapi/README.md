# Composable OpenAPI Layout

This directory provides a merged OpenAPI layout with shared components and path fragments.

## Source-of-Truth Rule
- Per-feature files in `SPEC/SPECKIT/specs/*/contracts/openapi.yaml` are the editable source.
- This merged layout is a derived tooling surface and should not be treated as the primary hand-edited source.

## Files
- `openapi.yaml`: root document
- `paths/`: path fragments grouped by bounded feature
- `components/parameters.yaml`: reusable parameters
- `components/schemas/*.yaml`: reusable schema groups

Use this layout when you want one merged spec surface for tooling while keeping feature-level contracts under `specs/*/contracts/openapi.yaml` as the editable source.
