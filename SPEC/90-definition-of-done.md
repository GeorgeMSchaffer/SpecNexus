# Definition of Done

## Engineering
- Feature behavior matches the relevant `SPEC/20-feature-*.md` file.
- Business logic is implemented in Application/Domain layers, not controllers or UI.
- No hardcoded credentials or secrets.
- API boundary validation and application business-rule validation follow `SPEC/30-Contracts.md`.

## Contracts
- `SPEC/30-Contracts.md` is updated when contracts change.
- OpenAPI/spec files are updated when endpoints or payloads change.
- Canonical `SPEC` docs are updated before syncing derivative `SPEC/SPECKIT` artifacts.

## Testing
- Acceptance criteria are covered by tests.
- Regression risk is covered by targeted tests.
- Development-only demo seed behavior is validated, including idempotency and required seeded graph.
- Non-Development runtime is validated to ensure demo seed does not run.

## Delivery
- Each PR links the feature spec it implements.
- Out-of-scope behavior is not added without approval.
- MVP release sign-off does not require OAuth or SAML endpoint delivery.