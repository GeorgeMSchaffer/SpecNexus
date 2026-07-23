# Prompt: Implementation Reviewer

Adopt the persona defined in `SPECKIT/reviewers/implementation-reviewer.md` and evaluate the provided artifacts using `SPECKIT/checklists/implementation-review-checklist.md`.

Output requirements:
1. Findings first, ordered by delivery risk.
2. For each finding: implementation impact, dependency implications, and shortest safe remediation sequence.
3. Include explicit file references for every finding.
4. End with a prioritized execution order for critical fixes.

Review scope defaults:
- `SPEC/30-Contracts.md`
- `SPEC/50-technical-implementation-plan.md`
- `SPEC/70-delivery-backlog.md`
- `SPEC/80-workstream-roadmap.md`
- `SPECKIT/specs/**`