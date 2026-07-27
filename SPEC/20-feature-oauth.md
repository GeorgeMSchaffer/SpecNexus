# Feature: OAuth (Phase 2, Post-MVP)

## Outcome
Organizations can enable Microsoft Entra ID sign-in while preserving existing local login behavior.

## Scope
- In (Phase 2): Microsoft Entra ID OAuth/OIDC integration, organization-scoped SSO configuration, callback handling, user linking/provisioning, and audit coverage.
- Out (MVP): OAuth/OIDC implementation is not part of MVP delivery.
- Out (Phase 2): SAML, MFA, social login providers, SCIM provisioning, and external logout propagation.

## Requirements
1. OAuth implementation is delivered in Phase 2 after MVP release.
2. Phase 2 provider target is Microsoft Entra ID.
3. Organizations must use organization-scoped sign-in entry points to initiate OAuth.
4. Local email/password login remains supported for coexistence and break-glass access.
5. Users are linked first by stored external identity mapping.
6. If no mapping exists, users are matched by globally unique email.
7. If no user exists, auto-provisioning creates a user in the initiating organization with default role `User`.
8. Inactive users remain blocked from authentication.
9. OAuth success and failure outcomes and provisioning events are audited.
10. OAuth configuration must not weaken global email uniqueness rules.

## Acceptance Criteria
- [ ] OAuth endpoints and callbacks are not required for MVP release completion
- [ ] Microsoft Entra ID sign-in works in Phase 2 using organization-scoped entry points
- [ ] Local login continues to work when OAuth is enabled
- [ ] Existing users can be linked by external identity and by email fallback
- [ ] Missing users can be auto-provisioned with default role `User`
- [ ] Inactive users are denied authentication
- [ ] OAuth outcomes and provisioning actions generate audit events