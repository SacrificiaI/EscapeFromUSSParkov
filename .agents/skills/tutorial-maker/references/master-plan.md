# Master Plan Specification

Use this reference when creating, compressing, reinforcing, or auditing a
canonical tutorial plan.

## The plan's job

The plan is an execution specification, not an essay, review transcript, or
bucket of possible ideas. It preserves the decisions a new author cannot safely
reconstruct from filenames: reader, progression, boundaries, contracts,
reference changes, source routing, validation, and deliberate omissions.

Keep rationale only where removing it would cause a plausible future author to
reverse or misapply the decision. State the current conclusion directly.

## Required sections

Adapt headings to local style, but cover these concerns:

1. **Goal and current state** — reader outcome, course questions, existing and
   remaining deliverables.
2. **Locations and scope** — canonical plan, tutorial root, reference project,
   repositories, and excluded artifacts.
3. **Reader and authoring intent** — prerequisites, depth, tone, running example,
   and the why-not-the-other-way test.
4. **Argument anchors** — load-bearing conclusions each chapter must preserve.
5. **Deliverable tree** — exact filenames, groupings, required/optional paths,
   index, glossary, and supporting artifacts.
6. **Authoring contract** — stable lesson format and proof standard.
7. **Sequence and prerequisite contract** — direct dependencies and required
   recall for every lesson.
8. **Execution blueprints** — exact H2 sequence and lesson-specific scope.
9. **Reference implementation contract** — codebase shape, dependency rules,
   version pins, build/run commands, generated-file policy, milestones, and
   immutable states when applicable.
10. **Technical contracts** — types, ranges, order of operations, ownership,
    external API constraints, or other facts later lessons must share.
11. **Per-lesson specifications** — outcome, implementation/reference delta,
    and verification for every file.
12. **Authoring schedule** — one bounded primary deliverable per session by
    default, plus required supporting edits.
13. **Source ledger and routing** — source role, version, claim, and lessons.
14. **Verification and acceptance** — structural, per-file, course-wide,
    reference-project, and reader-pilot checks.
15. **Decision log** — dated current decisions without retaining obsolete
    execution instructions.

Omit sections that genuinely do not apply. A prose-only conceptual series does
not need fake code tags; a build-along does need reproducible states.

## Per-lesson execution card

Every lesson specification must let a cold-start author answer the following
without inventing a course-level decision:

```text
Lesson and role:
Reader question:
Observable outcome:
Why it occurs here:
Minimum prerequisite and required recall:
Exact H2 sequence:
Files created:
Files edited:
Files deliberately unchanged:
Reference implementation state or tag:
Contracts, types, APIs, versions, and ranges introduced:
Implementation or reasoning order:
Expected proof and exact verification method:
Failure modes taught here:
Deliberate omissions and the lesson that owns them:
Required sources and glossary terms:
Done when:
```

Use “not applicable” only when it communicates a useful boundary. Do not pad a
prose lesson with invented files or commands.

## When to freeze code detail

Provide exact signatures, snippets, schemas, commands, or pseudocode when at
least one condition holds:

- multiple later lessons depend on the same contract;
- small differences change behavior or compatibility;
- an external framework requires matching names, paths, attributes, channels,
  serialization shapes, or lifecycle order;
- a weaker author is likely to create a second incompatible design;
- verification requires an exact input and expected output;
- the code demonstrates the course's central boundary or invariant.

Freeze behavior and evidence rather than incidental syntax when several clean
implementations are equally valid. Do not prescribe full code merely to make a
plan look detailed. Long copy-ready code belongs in the reference
implementation or a lesson, while the plan records the stable contract and
critical fragment.

## Reference implementation states

For a cumulative build-along, each lesson-changing state must be recoverable.
Use commits, tags, branches, snapshots, or archived fixtures according to the
repository workflow. A tag or state is valid only after the documented build,
tests, runtime check, and generated-file check succeed.

Record:

- toolchain and dependency pins;
- exact setup, restore, build, test, run, and manual verification commands;
- expected outputs or observable behavior;
- files the lesson adds or changes;
- how readers retrieve the matching state;
- `.gitignore` rules and teammate setup instructions for generated output;
- which parts require an editor, device, browser, server, database, or multiple
  processes and cannot be proved by a unit test alone.

Do not claim a snippet is working unless it exists in a verified state. Label
architecture sketches, pseudocode, and read-alongs accurately.

## Plan consistency checks

Search the full plan for competing answers to each disputed decision. Normalize
terms, versions, ranges, filenames, headings, prerequisites, counts, and status.

Verify:

- one canonical location and one active decision per issue;
- deliverable counts match the tree and schedule;
- every lesson's prerequisite exists and optional paths remain optional;
- headings in the blueprint match headings already published;
- technical contracts agree across cards, tables, and final specifications;
- each working code claim maps to a verified reference state;
- every accepted review correction is routed to a lesson or marked inapplicable;
- old plan copies are clearly backups, not competing instructions;
- the plan contains no drafting dialogue, unclosed proposals, or unexplained
  placeholders.

## Cold-start executor test

The plan is ready when an author unfamiliar with its drafting can take one
lesson card and determine:

1. what the reader must learn;
2. why the lesson occurs at that point;
3. what to read before writing;
4. what code or artifact may change;
5. which decisions are already settled;
6. how to prove the result;
7. what must remain for later lessons.

If the author must choose an architecture, invent an API, guess a version,
infer a prerequisite, or fabricate validation, reinforce the plan before
delegating the lesson.

