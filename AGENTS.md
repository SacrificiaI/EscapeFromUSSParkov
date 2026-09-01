# Agent Instructions

Read [`CLAUDE.md`](CLAUDE.md) completely before doing any work in this
repository. It is the canonical source for project context, architecture,
conventions, skills, verification commands, and scope decisions. If this file
conflicts with `CLAUDE.md`, follow `CLAUDE.md`.

The following operating principles supplement those project-specific rules.

## Think Before Coding

Do not silently choose an interpretation and run with it.

- State material assumptions explicitly.
- When genuine ambiguity would materially change the result, present the
  plausible interpretations or ask for clarification.
- Surface inconsistencies and concrete tradeoffs.
- Push back when the premise is wrong or a simpler approach exists.
- Stop and name what is unclear when proceeding would require guessing.

## Simplicity First

Write the minimum code that solves the requested problem.

- Add no unrequested features.
- Do not create abstractions for single-use code.
- Do not add speculative flexibility or configurability.
- Do not add handling for impossible scenarios.
- If a substantially smaller implementation would solve the same problem,
  simplify it.

Test: would a senior engineer call the result overcomplicated? If so, simplify
it.

## Surgical Changes

Touch only what the task requires and clean up only consequences of your own
changes.

- Do not improve adjacent code, comments, or formatting without being asked.
- Do not refactor unrelated code.
- Match the repository's established style.
- Mention unrelated dead code instead of deleting it.
- Remove imports, variables, functions, and files only when your change made
  them unused.

Test: every changed line should trace directly to the user's request.

## Goal-Driven Execution

Define verifiable success criteria and continue until they are satisfied.

- Turn a bug fix into a reproducing test followed by a passing fix when the
  behavior can be tested at the appropriate layer.
- Turn validation work into invalid-input tests followed by the implementation.
- For refactors, establish that relevant checks pass before and after when
  practical.
- For multi-step tasks, state a brief plan in this form:

```text
1. [Step] -> verify: [check]
2. [Step] -> verify: [check]
3. [Step] -> verify: [check]
```

Use the build, test, and manual verification requirements defined in
`CLAUDE.md`; do not invent weaker substitutes.
