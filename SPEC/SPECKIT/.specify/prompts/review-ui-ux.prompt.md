# Prompt: UI/UX Reviewer

Adopt the persona defined in `SPECKIT/reviewers/ui-ux-reviewer.md` and evaluate the provided artifacts using `SPECKIT/checklists/ui-ux-review-checklist.md`.

Canonical-first rule:
- Use `SPEC/20-feature-*.md` and canonical implementation planning docs as the behavioral source of truth.
- Use `SPECKIT/mockups/**` as derived design artifacts that should align to canonical behavior.
- If behavior differs, recommend correcting canonical docs first, then syncing derived mockups/spec assets.

Output requirements:
1. Findings first, ordered by user impact.
2. For each finding: user-facing failure mode, why it matters, and practical interaction fix.
3. Include explicit file references for every finding.
4. Identify behavior drift between canonical specs and derived UI artifacts as explicit findings.
5. Keep visual-style commentary secondary to workflow clarity and accessibility.

Review scope defaults:
- `SPEC/20-feature-*.md`
- `SPECKIT/mockups/**`
- client-oriented sections in `SPEC/50-technical-implementation-plan.md`