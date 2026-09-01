# Curriculum Design

Use this reference when defining a new series, reviewing lesson order, or
repairing prerequisites and learning progression.

## Start from competence

Write a compact course contract before naming files:

- **Reader:** knowledge and tools already assumed.
- **Exit competence:** tasks the reader can perform unaided after completion.
- **Boundary:** adjacent topics explicitly included, deferred, or excluded.
- **Transfer test:** a new situation where the reader must apply the reasoning
  instead of copying the running example.
- **Stopping points:** useful completion states for readers who do not need
  optional or advanced branches.

Turn the exit competence into three to seven questions the finished course must
answer. These questions are the stable curriculum spine. A lesson exists only
when it advances one of them.

## Select the source mode

Use one or combine them deliberately:

| Mode | Implementation truth | Curriculum evidence | Main risk |
|---|---|---|---|
| Codebase-derived | Current source, build, tests, configuration | Real dependency and request/gameplay flow | Teaching accidental inconsistencies as intentional design |
| Topic/reference-driven | Verified reference implementation | Primary docs, specifications, canonical books/papers | Inventing code that was never executed |
| Validated tutorial synthesis | Reproduced examples and primary docs | Strong tutorials, books, and courses | Copying outdated claims or another author's structure |
| Hybrid | Codebase plus verified extensions | All applicable sources, with roles recorded | Mixing current behavior with aspirational architecture |

For codebase-derived work, find the actual entry points and manifests for the
technology rather than assuming a framework-specific filename. Trace one
complete path through the system. Read tests and configuration because they
often define behavior more precisely than prose or class names.

## Use a concrete-first spiral

A strong default sequence is:

1. **Mental model and adoption decision** — definition, purpose, costs, and the
   cases where the topic is not useful.
2. **Small complete rehearsal** — build or inspect the smallest end-to-end slice
   that exposes the central decision.
3. **General classification** — extract reusable rules from the rehearsal.
4. **Environment and lifecycle constraints** — revisit the same example under
   the host framework, runtime, timing, persistence, or integration rules.
5. **Adjacent approaches** — separate orthogonal concerns and live alternatives
   without creating a false maturity ladder.
6. **Failure diagnosis** — symptom, cause, proof, and fix for realistic errors.
7. **Optional advanced branch** — production scale, networking, performance,
   security, distributed systems, or other costs not every reader needs.
8. **Transfer and capstone** — apply the decisions to a different vertical slice
   and prove the reader can explain the result.

Change this sequence when the topic has a different dependency graph. Preserve
the principle: experience before taxonomy, prerequisites before dependents, and
optional complexity after a useful stopping point.

## Build the lesson graph

For each proposed lesson, record:

- the single reader question it answers;
- one observable outcome;
- its minimum direct prerequisite and required recall;
- whether it is core, optional, alternative, reference, drill, or capstone;
- what new code/state/artifact it introduces;
- what later lesson depends on it.

Then audit the graph:

- A required lesson never depends on an optional lesson.
- A chapter spine carries the reusable rule, quick-reference table, and map;
  sub-lessons apply or deepen it without re-arguing the spine.
- A lesson is split when the reader's question changes, not at an arbitrary
  length.
- A lesson stops once its question is answered.
- Terms appear after the experience needed to understand them, unless the term
  itself is necessary to complete the rehearsal.
- Failure modes appear no later than the point where readers can realistically
  create them.
- Advanced requirements do not leak backward and distort the basic path.

## Choose the worked example

Prefer one cumulative example when lessons modify code or configuration. It
must be small enough that new lessons teach the topic rather than content entry,
yet rich enough to survive later constraints.

Freeze:

- the domain object or vertical slice;
- naming and folder/namespace conventions;
- the initial behavior contract;
- the files expected to grow across lessons;
- the verification surface: tests, commands, observable UI, requests, logs, or
  serialized output.

Use smaller independent examples for conceptual-only material when a cumulative
project would add ceremony without improving transfer. State that choice in the
plan so later authors do not invent a second architecture.

## Design exercises for retrieval and transfer

Exercises come after the complete worked pattern. Default to one exercise per
lesson and two only when the lesson contains two genuinely independent skills.

Use three levels across a course:

1. **Reproduction:** rebuild a just-completed pattern without copying.
2. **Variation:** change one constraint while keeping the rule.
3. **Transfer:** classify or implement a new case without answer-shaped hints.

Provide a checkable solution or rubric. For a diagnostic drill, hide placements
or causes until the reader commits to an answer.

## Sequence audit

Before freezing filenames and headers, ask:

- Can the reader explain why every lesson occurs now?
- Does each lesson consume something previously established?
- Is there visible working evidence early and often?
- Are terminology and abstraction introduced after a concrete need?
- Does every optional branch have a clean entry and return path?
- Can a reader stop at each declared stopping point with a complete, honest
  mental model?
- Does the capstone test reasoning rather than memory of the running example?

