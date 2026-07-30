---
name: decision-interview
description: 'Turn ambiguous requirements into a guided, one-question-at-a-time interview so decisions are explicit before implementation.'
license: MIT
---

# Decision Interview Skill

Use this skill when a feature or workflow has unclear requirements that should be resolved before implementation. This skill converts ambiguous requirements into a structured interview flow with one question at a time, concrete answer choices, and a decision log that can be handed to product, engineering, or security reviewers.

## When to Use

Use this skill when you need to:
- Clarify ambiguous requirements before coding
- Resolve workflow or state-transition edge cases
- Surface concurrency risks and define handling rules
- Identify security or prompt-injection concerns early
- Avoid making implementation decisions silently

## Core Workflow

### 1. Review the relevant context

Start from the spec, backlog item, or implementation notes and identify:
- ambiguous requirements
- state transition edge cases
- concurrency problems
- security or trust-boundary concerns

### 2. Propose one concrete clarification per issue

For each issue, write one explicit recommendation such as:
- a new workflow state
- a specific timeout behavior
- a concurrency rule
- a security restriction

Do not silently decide. Make the proposal explicit and frame it as something the approver must confirm.

### 3. Turn each issue into an interview question

For every clarification, ask one question with a small set of answer choices. Prefer:
- yes/no choices
- role-based choices
- behavior-based choices

The goal is to keep the conversation simple and sequential.

### 4. Ask one question at a time

Present the next question only after the previous answer is captured. Do not batch questions unless the user explicitly requests it.

### 5. Record the approved decisions

After the interview, summarize the decisions in a compact decision log:
- Decision 1: [selected option]
- Decision 2: [selected option]
- Decision 3: [selected option]

### 6. Convert the decisions into implementation-ready guidance

Use the approved decisions to produce:
- a spec-ready clarification list
- a workflow rule set
- acceptance criteria updates
- implementation notes for engineering

## Decision Points

- If the requirement is already clear, document it and move on.
- If the behavior affects state changes, approvals, persistence, or security, force a decision.
- If the user prefers an option not listed, rewrite the choice to fit and continue.
- If the decision affects downstream implementation, capture it in a decision log immediately.

## Quality Criteria

A good interview result should:
- surface unresolved ambiguity instead of hiding it
- propose one concrete clarification per issue
- avoid silent assumptions
- keep the user in control of the final decision
- produce a clear record that can be used in specs or implementation planning

## What This Skill Produces

This skill produces a lightweight, approval-driven decision framework that helps teams move from vague requirements to explicit product or engineering choices before implementation begins.

## Example Prompts

- "Interview me on the approval workflow decisions for this feature."
- "Help me clarify the ambiguous requirements for this workflow before implementation."
- "Turn these spec gaps into one-question-at-a-time interview choices."
- "Identify the missing decisions around state transitions, concurrency, and security for this feature."

## Related Customizations

Good next customizations to create:
- a spec-gap review skill
- a workflow-state design skill
- a decision-log template skill
