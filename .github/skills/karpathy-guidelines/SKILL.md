---
name: karpathy-guidelines
description: 'Behavioral guidelines to reduce common LLM coding mistakes. Use when writing, reviewing, or refactoring code to avoid overcomplication, make surgical changes, surface assumptions, and define verifiable success criteria.'
license: MIT
---

# Karpathy Guidelines

Behavioral guidelines to reduce common LLM coding mistakes, derived from [Andrej Karpathy's observations](https://x.com/karpathy/status/2015883857489522876) on LLM coding pitfalls.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## When to Use

Use this skill when you are:
- Writing new code
- Reviewing code
- Refactoring existing code
- Debugging a bug that needs a minimal, defensible fix
- Deciding whether a proposed change is too complex for the request

## Core Workflow

### 1. Think Before Coding

Do not assume. Surface confusion early.

Before implementing:
- State your assumptions explicitly.
- If multiple interpretations exist, name them.
- If something is unclear, stop and ask.
- If a simpler approach exists, say so.
- Push back when a requested change looks overcomplicated for the goal.

Decision point:
- If the request is ambiguous, clarify before editing.
- If the request is clear, proceed with the smallest defensible change.

### 2. Simplicity First

Choose the minimum code that solves the problem.

Rules:
- Do not add features beyond the request.
- Do not introduce abstractions for one-off code.
- Do not add configurability that was not requested.
- Do not add error handling for impossible scenarios.
- If the implementation could be much shorter, simplify it.

Decision point:
- If a proposed design needs extra scaffolding, try the direct version first.
- If the direct version is enough, keep it.

### 3. Make Surgical Changes

Touch only what you must.

When editing existing code:
- Do not refactor unrelated code.
- Do not rewrite comments, formatting, or names unless needed for the task.
- Keep the existing style.
- Remove only imports, variables, or helpers made unused by your own change.
- Do not delete pre-existing dead code unless asked.

Decision point:
- If a change affects more files than necessary, reduce the blast radius.
- If a nearby simplification does not help the task, leave it alone.

### 4. Define Success Up Front

Turn the task into something you can verify.

Examples:
- Add validation -> write a failing test, then make it pass.
- Fix a bug -> reproduce it, then verify the fix.
- Refactor code -> ensure behavior stays the same.

For multi-step work, state a short plan:

```text
1. [Step] -> verify: [check]
2. [Step] -> verify: [check]
3. [Step] -> verify: [check]
```

Success criteria:
- The change matches the request exactly.
- The code is as simple as possible.
- Any touched behavior is verified.
- No unrelated code changed.

## Review Checklist

Use this checklist before finishing:
- Did I state my assumptions or ask about unknowns?
- Did I choose the simplest implementation that works?
- Did I keep the change surgical?
- Did I define and verify success?
- Did I avoid speculative flexibility?
- Did I remove only the code I made obsolete?

## What This Skill Produces

This skill helps you produce small, explicit, verifiable changes instead of speculative refactors. It is meant to keep the work grounded in the request and to prevent accidental overengineering.

## Example Prompts

- "Refactor this function with the Karpathy Guidelines."
- "Review this change using the Karpathy Guidelines and call out overengineering risk."
- "Fix the bug with the smallest possible change and verify it."
- "Rewrite this implementation to be simpler without changing behavior."

## Related Customizations

Good next customizations to create:
- A review checklist skill for bug hunting and regression risk
- A test-first debugging skill for reproducing failures before editing
- A change-scoping skill for keeping edits minimal across a workspace
