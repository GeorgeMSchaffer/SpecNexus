# Research: Organizations and Users

## Key Decisions
- organizations are archived, not hard-deleted
- new organizations receive default statuses and one default board
- user emails are globally unique across the system
- user lifecycle states are `Active` and `Inactive`
- all admin and collaboration mockups use a unified shell with header, role-aware navigation, and breadcrumbs
- organization edit supports logo upload with immediate thumbnail preview
- each organization has one active logo; new upload replaces the previous asset
- organization logo rendering is constrained to a maximum height of `150px`

## Risk Areas
- bootstrapping organizations atomically with default data
- preventing last-Org-Admin lockout scenarios
- preserving auditability of user role and status changes
- ensuring logo replacement is atomic so stale logo references do not remain in cache
- maintaining predictable breadcrumb patterns across admin and board surfaces

## Open Questions
- Should logo upload be limited to PNG and SVG only, or allow JPEG and WebP as well?
- Should there be a max file size limit (recommended: `2 MB`) in MVP?
- Should the API expose versioned logo URLs for cache busting on replacement?
