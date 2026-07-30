# Canonical Sync Workflow

## Purpose
Define a deterministic process for synchronizing derived `SPEC/SPECKIT` artifacts from canonical `SPEC` sources.

## Precedence Rule
- Canonical behavior authority is `SPEC/*.md`.
- `SPEC/SPECKIT/**` artifacts are derived and must not introduce net-new behavior.
- If drift is found, update canonical docs first, then sync derived artifacts.

## Canonical to Derived Mapping
| Canonical Source | Derived Targets | Notes |
|---|---|---|
| `SPEC/10-requirements.md` | `SPEC/SPECKIT/specs/002-006/spec.md` | Global and cross-feature behavior is distributed into feature specs. |
| `SPEC/20-feature-auth.md` | `SPEC/SPECKIT/specs/002-authentication-and-access/spec.md` | Authentication and access behavior. |
| `SPEC/20-feature-organizations-and-users.md` | `SPEC/SPECKIT/specs/003-organizations-and-users/spec.md` | Tenant administration behavior. |
| `SPEC/20-feature-boards-and-statuses.md` | `SPEC/SPECKIT/specs/004-boards-and-statuses/spec.md` | Board and status lifecycle behavior. |
| `SPEC/20-feature-ideas-and-engagement.md` | `SPEC/SPECKIT/specs/005-ideas-and-engagement/spec.md` | Collaboration and idea behavior. |
| `SPEC/20-feature-notifications.md` | `SPEC/SPECKIT/specs/006-notifications-and-audit/spec.md` | Notification and audit behavior scope. |
| `SPEC/30-Contracts.md` | `SPEC/SPECKIT/specs/*/contracts/openapi.yaml`, `SPEC/SPECKIT/openapi/openapi.yaml` | Contracts are derived for tooling and review. |
| `SPEC/40-test-strategy.md` | `SPEC/SPECKIT/specs/*/tasks.md`, review checklists | Verification expectations and test coverage mapping. |
| `SPEC/50-technical-implementation-plan.md` | `SPEC/SPECKIT/specs/*/plan.md` | Execution sequencing and architecture intent. |
| `SPEC/70-delivery-backlog.md`, `SPEC/80-workstream-roadmap.md` | `SPEC/SPECKIT/specs/*/tasks.md`, `SPEC/SPECKIT/specs/*/plan.md` | Delivery slicing and milestone alignment. |
| `SPEC/90-definition-of-done.md` | `SPEC/SPECKIT/checklists/*.md`, `SPEC/SPECKIT/review-workflow.md` | Quality and sign-off gates. |

## Required Sync Steps
1. Edit canonical source files in `SPEC/`.
2. Identify impacted feature streams (`002` through `006`) and artifact types (`spec`, `plan`, `tasks`, `contracts`, checklists).
3. Update derived artifacts in `SPEC/SPECKIT` to reflect canonical behavior.
4. Update derived metadata (`Status`, `Canonical Sources`, `Last Canonical Sync Date`) in each impacted derived artifact that includes metadata headers.
5. Run review workflow drift gate from `SPEC/SPECKIT/review-workflow.md`.
6. Block sign-off if any impacted derived artifact remains stale.

## Drift Gate Checklist
- Canonical source files are updated for every behavior change.
- No derived artifact contains behavior absent from canonical docs.
- Each impacted derived artifact with metadata headers has current sync metadata.
- Contract behavior in derived OpenAPI aligns with `SPEC/30-Contracts.md`.
- Deferred scope remains clearly deferred in both canonical and derived docs.
- Review findings document canonical-first remediation when drift is present.

## Automation
- Pull requests run `.github/workflows/spec-drift-gate.yml`.
- CI validation logic is implemented in `scripts/spec_drift_gate.ps1`.

## Pilot Recommendation
Use `002-authentication-and-access` as the first full sync pilot before applying this workflow to remaining feature streams.

## Future Scope Note
- `007-kubernetes-containerization` is future/non-MVP exploratory scope and is excluded from the active MVP sync gate unless canonical future-scope planning docs are explicitly updated.
