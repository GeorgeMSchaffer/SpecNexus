# Remaining Spec Q&A Backlog

## Purpose
Track only unresolved clarification items that still affect future scope or incomplete contract details.

## Remaining MVP Clarifications

### 1. Validation message conventions
Question: What standard validation message patterns should the API and UI use for required fields, max lengths, and invalid formats?
Why it matters: the field-level limits are now defined, but consistent validation messaging would further reduce implementation drift.

Resolved field decisions:
- user email is globally unique across the system
- comment body max length is 2000 characters
- comment bodies are plain text and may include line breaks
- first and last name max length is 100 characters
- company name max length is 200 characters
- address max length is 200 characters
- city max length is 100 characters
- state max length is 50 characters
- zip max length is 20 characters
- phone max length is 25 characters
- organization and user text fields are trimmed before validation and persistence
- archived organizations are hidden by default unless explicitly filtered in
- soft-deleted statuses keep their prior name with an archived or deleted label in historical or detail views
- audit events include both human-readable messages and structured metadata
- audit and notification events do not require read or query endpoints in MVP
- temporary passwords are one-time display, expire after 24 hours, and force password change on first use
- unresolved mentions show inline validation and block save
- comment entry uses a live character counter and inline overflow validation
- board and admin views use guided empty states with a primary action and short explanatory text
- event verification uses tests and internal diagnostics outside the public API surface

## Later Decisions

### 2. Reporting requirements
Question: What reports, metrics, and export formats will reporting need when it is specified later?
Why it matters: some data retention and audit decisions are easier to make now than retrofit later.

### 3. OAuth and SSO direction
Question: Which identity providers and account-linking rules should future OAuth support follow?
Why it matters: choosing stable user identifiers now will reduce migration risk later.

### 4. Attachments and rich content
Question: Are attachments, rich text, or embedded links expected in future idea or comment workflows?
Why it matters: content storage and sanitization approaches differ if richer content is expected later.

## Recommended Resolution Order
1. Finish the remaining field-level validation rules needed for complete OpenAPI and UI contracts.
2. Resolve reporting expectations before designing export-friendly data views.
3. Resolve future OAuth and rich-content direction before expanding identity or content models.
