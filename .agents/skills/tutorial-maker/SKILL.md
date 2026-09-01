---
name: tutorial-maker
description: >
  Design, plan, author, verify, or repair a coherent programming tutorial
  series. Use for codebase-derived courses, conceptual curricula, validated
  syntheses of existing tutorials, master plans, individual lessons, course
  audits, and execution handoffs. Do not use for a short one-off explanation
  that does not need a maintained curriculum.
argument-hint: "Topic, codebase, plan, lesson, or tutorial-series task"
user-invocable: true
---

# /tutorial-maker

**Portable, project-agnostic skill.** Everything required to apply the workflow
is contained in this skill folder. Inspect and follow the target repository's
own instructions and established tutorial format, but do not depend on files
from the repository that hosts this copy of the skill.

## Outcome

Produce a tutorial set that a defined reader can follow from known prerequisites
to observable competence without live rescue. Keep the curriculum coherent,
the technical claims current, the examples executable, and the plan precise
enough for a cold-start author to continue without inventing course-level
decisions.

## Choose the operating mode

Infer the smallest mode that satisfies the request:

1. **Discover and design** — define a new tutorial set and its curriculum.
2. **Plan** — create or reinforce the canonical master plan.
3. **Author** — write or revise one lesson from an approved plan.
4. **Audit** — evaluate sequence, pedagogy, technical accuracy, or execution
   readiness; edit only when authorized.
5. **Handoff** — give another author an execution-safe assignment for one
   lesson or bounded group.

For a new tutorial set, ask which repository or folder will own it before
creating files unless the caller already supplied an exact destination. Never
guess a writable destination, create a second plan beside an unknown canonical
one, or move a course between repositories implicitly.

Read only the references required for the chosen mode:

- New series or sequence work: [curriculum-design.md](references/curriculum-design.md)
- Creating or reinforcing a plan: [master-plan.md](references/master-plan.md)
- Writing or revising a lesson: [lesson-authoring.md](references/lesson-authoring.md)
- Internet research, fact-checking, audits, or handoffs:
  [research-review-handoff.md](references/research-review-handoff.md)

Read all four when creating a complete new course plan. Do not load every
reference for a narrow single-lesson edit.

## Intake

Discover answers from the destination, existing files, and request before
asking questions. Ask only for choices that materially change the result.

Establish:

- exact destination and canonical plan path;
- topic and course boundary;
- reader baseline and intended exit competence;
- source mode: codebase-derived, topic/reference-driven, validated tutorial
  synthesis, or hybrid;
- required output format and local naming/navigation conventions;
- whether one cumulative reference implementation is justified;
- pinned languages, frameworks, engines, SDKs, tools, and platforms;
- required versus optional paths, publication target, and verification access.

If the caller accepts defaults, use these:

- adapt to the destination's existing format; otherwise use polished Markdown
  suitable for an Obsidian vault;
- use one cumulative worked example when the topic benefits from implementation;
- require current primary documentation for checkable technical claims;
- use reputable tutorials and courses as pedagogy and curriculum evidence only
  after validating every adopted technical claim;
- make optional or advanced branches explicit and keep them out of the required
  prerequisite chain;
- design plans so a less capable author can execute one lesson without making
  architecture, API, sequencing, or validation decisions.

## Source modes

### Codebase-derived

Inspect the real build manifests, entry points, dependency graph, configuration,
tests, and the running example's end-to-end path before designing lessons. Use
the codebase's actual versions and conventions. Do not manufacture lessons for
features the codebase does not contain unless the requested course explicitly
teaches a greenfield extension.

### Topic/reference-driven

Derive the curriculum from the learner outcome, current primary documentation,
and a deliberately designed reference implementation. Separate factual
contracts from architectural judgment. Make every architectural judgment expose
its deciding constraint.

### Validated tutorial synthesis

Existing tutorials, books, certified courses, and high-quality training paths
may shape pacing, explanations, exercises, and sequence. Validate the specific
material used; reputation alone does not make an individual claim current.
Check adopted APIs, commands, versions, and behavior against primary sources or
an executable reference. Synthesize the teaching approach in original prose;
do not copy distinctive explanations, examples, exercises, or course structure.

### Hybrid

Use the codebase as implementation truth, primary sources as technical truth,
and validated educational sources as evidence about how to teach the material.
Record each source's role so those forms of authority are never conflated.

## Non-negotiable teaching standard

- Define one reader. Do not write simultaneously for a beginner and an expert.
- State what the reader can do at the end, not merely what the files contain.
- Put a concrete problem or observable need before the abstraction that solves
  it.
- Prefer a concrete-first spiral: build or inspect a small complete slice,
  extract the general rule, revisit it under harder constraints, diagnose its
  failures, then transfer it to a new case.
- Give each lesson one dominant reader question and one observable outcome.
- Reuse one running example instead of inventing disposable parallel examples.
- Apply the **why-not-the-other-way test** at each real choice. If the intended
  reader could reasonably choose the alternative, answer why in one to three
  sentences at the decision point.
- Distinguish requirements, defaults, optional choices, alternatives, and
  deliberate omissions. Do not present preferences as laws.
- Every implementation stage follows: target behavior, focused explanation,
  implementation, exact proof.
- Every prose-only stage follows: claim, trace/classification/comparison,
  checked conclusion.
- Examples claimed to work must compile, run, or otherwise be verified in the
  real environment. Mark pseudocode and unverified sketches honestly.
- Preserve one authoritative current decision. Remove review transcripts,
  superseded proposals, and drafting history from execution instructions.

## Scope and permission boundaries

- Treat review, critique, and verification requests as read-only unless editing
  is also requested.
- Author one planned lesson at a time unless the caller explicitly requests a
  larger batch. Update only its required glossary, index, reference example, or
  plan dependencies.
- Do not rename, merge, split, or add primary lessons when a canonical plan has
  frozen the course tree without revising that plan and its affected links.
- Do not commit, tag, push, publish, buy course access, or message reviewers
  without explicit authorization.
- Preserve unrelated files and existing worktree changes.
- If implementation exposes a wrong plan contract, correct the canonical plan
  before or with the lesson. Do not silently diverge from it.

## Completion report

Report:

- files created or changed;
- operating mode and source mode used;
- validation performed and any checks that require a human or unavailable tool;
- unresolved factual or curriculum decisions;
- the exact next lesson or planning action, when a sequence continues.

