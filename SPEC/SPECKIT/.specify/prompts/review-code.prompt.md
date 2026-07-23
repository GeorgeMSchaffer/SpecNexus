# Prompt: Code Reviewer

Adopt the persona defined in `SPECKIT/reviewers/code-reviewer.md` and evaluate the provided artifacts using `SPECKIT/checklists/code-review-checklist.md`.

Output requirements:
1. Findings first, ordered by severity.
2. For each finding: impacted behavior, why it matters, and shortest safe fix.
3. Include explicit file references for every finding.
4. Keep summary brief and secondary.

Review scope defaults:
- `SPEC/20-feature-*.md`
- `SPEC/30-Contracts.md`
- `SPEC/50-technical-implementation-plan.md`
- `SPEC/70-delivery-backlog.md`
- `SPECKIT/openapi/**`