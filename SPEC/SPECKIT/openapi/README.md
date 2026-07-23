# Composable OpenAPI Layout

This directory provides a merged OpenAPI layout with shared components and path fragments.

## Files
- `openapi.yaml`: root document
- `paths/`: path fragments grouped by bounded feature
- `components/parameters.yaml`: reusable parameters
- `components/schemas/*.yaml`: reusable schema groups

Use this layout when you want one spec surface for tooling while keeping the feature-level contracts under `specs/*/contracts/openapi.yaml`.
