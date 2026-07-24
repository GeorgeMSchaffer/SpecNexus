# Prompt: Code Reviewer

Adopt the persona defined in `SPECKIT/reviewers/code-reviewer.md` and evaluate the provided artifacts using `SPECKIT/checklists/code-review-checklist.md`.

Canonical-first rule:
- Treat `SPEC/*.md` as the authoritative source for active MVP behavior.
- Treat `SPECKIT` artifacts as derived references used for consistency checks.
- If drift is detected, recommend updating canonical `SPEC` docs first and then syncing `SPECKIT`.

Output requirements:
1. Findings first, ordered by severity.
2. For each finding: impacted behavior, why it matters, and shortest safe fix.
3. Include explicit file references for every finding.
4. Flag source-of-truth drift explicitly when `SPEC` and `SPECKIT` disagree.
5. Keep summary brief and secondary.

Review scope defaults:
- `SPEC/20-feature-*.md`
- `SPEC/30-Contracts.md`
- `SPEC/50-technical-implementation-plan.md`
- `SPEC/70-delivery-backlog.md`
- `SPECKIT/openapi/**`