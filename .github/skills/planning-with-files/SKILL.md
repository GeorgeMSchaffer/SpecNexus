---
name: planning-with-files
description: Use when user explicitly requests planning with files, or asks for persistent tracking of a complex task.
---

# Planning with Files

Use persistent markdown files as external memory for complex tasks. Files survive context limits and session boundaries.

Use `TodoWrite` to decompose phases into executable tasks. Before closing session, use `Question` to confirm findings, decisions, and deliverables are satisfactory.

## When to Use

- User explicitly requests planning/tracking with files
- Complex multi-session tasks
- Research-heavy work requiring persistent notes
- Tasks where decisions and rationale need to be preserved

## When NOT to Use

- Simple single-session tasks (use Plan agent instead)
- Quick fixes or small changes
- Tasks with clear requirements needing no research

## Directory Convention

All planning files go in `_plans/` with date-prefixed task folders:

```
_plans/
  2026-01-08-dark-mode-toggle/
    plan.md
    research.md
    decisions.md
  2026-01-09-api-auth-refactor/
    plan.md
    scratch.md
```

**Naming:** `YYYY-MM-DD-task-name` (kebab-case, concise description)

## File Types

Create files based on what the task needs, not a fixed pattern:

| Type | When to Create | Content |
|------|----------------|---------|
| `plan.md` | Always | Goal, phases, status, errors encountered |
| `research.md` | Research-heavy tasks | Sources, findings, synthesized data |
| `decisions.md` | Tradeoff decisions | ADRs, options considered, rationale |
| `scratch.md` | Complex reasoning | Drafts, working notes, exploratory code |
| `[deliverable].md` | Final outputs | Reports, summaries, documentation |

### plan.md Template

```markdown
# Plan: [Brief Description]

## Goal
[One sentence describing the end state]

## Phases
- [ ] Phase 1: [Description]
- [ ] Phase 2: [Description]
- [ ] Phase 3: [Description]

## Status
**Current:** [What's happening now]

## Decisions
- [Decision]: [Rationale]

## Errors Encountered
- [Error]: [Resolution]
```

### research.md Template

```markdown
# Research: [Topic]

## Sources
- [Source]: [Key findings]

## Findings
### [Category]
- [Finding]
```

### decisions.md Template

```markdown
# Decisions: [Task]

## [Decision Title]
**Status:** Decided | Pending
**Options:**
1. [Option A] - [Pros/Cons]
2. [Option B] - [Pros/Cons]

**Choice:** [Selected option]
**Rationale:** [Why]
```

## Task System Integration

Planning files are for investigation; TodoWrite tasks are for execution.

**Workflow:**
1. Create `_plans/` directory and `plan.md`
2. Research and document findings
3. Make decisions, document rationale
4. Decompose phases into executable TodoWrite tasks

## Recommended Tools

| Tool | When to Use |
|------|-------------|
| **Explore subagent** | Search codebase, find patterns, understand existing structure before planning |
| **Question** | Clarify requirements, validate assumptions, confirm decisions before proceeding |

Use these proactively during investigation to avoid wrong assumptions and wasted effort.

## Success Criteria

Planning is complete when:
- [ ] `plan.md` exists with clear goal and phases
- [ ] All phases marked complete or explicitly deferred
- [ ] Key decisions documented with rationale
- [ ] Errors encountered are logged with resolutions
- [ ] User confirms deliverables via `Question`

## Critical Rules

### Refresh Goals When Context Gets Long

After many tool calls (~20+), re-read `plan.md` before major decisions. This brings goals back into the attention window.

### 1. Store, Don't Stuff
Large outputs go to files, not context. Keep paths in working memory, content in files.

### 2. Log All Errors
Every error goes in plan.md under "Errors Encountered". This builds knowledge and shows recovery.

### 3. Decisions Need Rationale
Don't just record what you decided - record WHY. Future-you needs this context.

### 4. Update Status Immediately
Mark phases complete as soon as they're done. Don't batch status updates.

## Anti-Patterns

| Don't | Do Instead |
|-------|------------|
| Create files in project root | Use `_plans/YYYY-MM-DD-name/` |
| State goals once and forget | Re-read plan.md when context is long |
| Hide errors and retry silently | Log errors with resolution |
| Stuff everything in context | Store large content in files |
| Start executing immediately | Create plan.md first for complex tasks |

## See Also

- [reference.md](reference.md) - Manus context engineering principles
- [examples.md](examples.md) - Real task examples with different file combinations