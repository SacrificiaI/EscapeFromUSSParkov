# Lesson Authoring

Use this reference to write or revise one lesson from an approved curriculum.

## Read before writing

Read:

1. the destination's governing instructions and existing format;
2. the canonical plan's course contract and this lesson's execution card;
3. the chapter spine and immediate prerequisite lesson;
4. every reference-implementation file this lesson reads or changes;
5. relevant tests, configuration, manifests, and current build/run instructions;
6. the primary documentation and routed educational sources required for the
   lesson's claims.

If code exists, establish a clean baseline with the relevant build, tests, and
runtime check before editing. Record unrelated failures instead of silently
teaching around them.

## Default lesson shape

Adopt the existing course format when one exists. Otherwise use this adaptable
Markdown shape:

1. frontmatter appropriate to the publication system;
2. H1 title;
3. table of contents containing major sections;
4. short abstract stating an observable learning outcome;
5. minimum direct prerequisites;
6. files or artifacts created and edited, omitted for prose-only lessons;
7. concrete problem or target behavior;
8. concept and numbered implementation/reasoning stages;
9. complete current shape of each multi-touch file after focused snippets;
10. one retrieval or transfer exercise with a checkable solution;
11. scannable summary of rules, decisions, commands, or checks;
12. short conclusion naming the next unanswered question or stopping point;
13. further reading containing sources actually used and any reference state;
14. version or last-verified stamp when the publication system uses one.

Indexes, glossaries, drills, chapter maps, and lookup documents may omit sections
that would be artificial. Document role-specific exceptions in the master plan.

## Teaching loop

For code-changing stages, use this order:

1. State the expected behavior or evidence.
2. Explain the one concept needed for the change.
3. Give the exact file location and implementation.
4. Run the smallest meaningful verification.
5. State the expected result and what a mismatch means.

For prose-only stages:

1. State a falsifiable claim or decision.
2. Trace a flow, classify cases, compare live alternatives, or inspect evidence.
3. Apply the decision to the running example.
4. End with a checked conclusion or transfer question.

Aim for focused explanatory blocks between code samples. Do not force a fixed
paragraph count when a safety constraint or difficult model needs more depth.

## Explanation standard

- Give the “why” where a realistic alternative exists, then return to the
  implementation. Do not annotate self-evident language mechanics.
- Define the operative rule before listing examples. Inventories and tables
  support the rule; they do not replace it.
- Name the deciding constraint for every recommendation. State the trigger that
  would change the choice.
- Separate orthogonal decisions instead of presenting them as stages on a
  maturity ladder.
- Attribute costs to the design choice that incurs them. Do not promise that an
  architecture removes work it merely relocates.
- Teach partial adoption and useful stopping points when the topic supports
  them.
- Keep advanced concerns out of earlier lessons unless they affect the earlier
  contract now.

## Voice

Write as a senior instructor producing a standalone lesson:

- direct, precise, and confident;
- no references to the requester, drafting process, model, or prior revisions;
- no changelog at the top and no “this used to be” narration in greenfield
  lessons;
- no hedging filler, cheerleading, or generic storytelling;
- no comparison to unused alternatives unless the distinction is necessary to
  use the chosen approach correctly;
- honest comparison tables when multiple options are genuinely live;
- exact failure symptoms rather than vague warnings.

Editing an existing file is normal. Tell the reader what to add now without
narrating the tutorial's revision history. A real failure mode remains valuable:
show symptom, cause, proof, and fix as a reusable warning.

## Code and artifact rules

- Use code actually read from or applied to the verified reference state.
- Match the target's naming, formatting, dependency, and project conventions.
- Use exact installed versions from manifests or pinned setup instructions.
- Provide complete namespaces/imports and access modifiers when the language
  requires them.
- State deliberate omissions and route them to the lesson that completes them.
- When a file grows across lessons, show the complete shape valid at the end of
  the current lesson without future members.
- Avoid repeated installation commands. Confirm dependencies introduced by an
  earlier lesson; install only genuinely new ones.
- Prefer the project's native verification surface: its test runner, request
  files, editor, emulator, CLI, browser, or runtime. Offer secondary tools only
  as fallbacks.
- Never replace a runtime or integration check with a unit test that cannot
  observe the behavior being claimed.

Pseudocode is allowed when implementation would distract from the lesson or the
plan declares a read-along. Label it visibly and do not claim it compiles.

## Callouts and scope notes

Use the publication system's established callouts. If none exist, use plain
Markdown blockquotes with consistent labels.

- **Abstract:** what the reader can do afterward.
- **Note / Files Touched:** created versus edited artifacts.
- **Info:** a deliberate omission and where it is completed.
- **Tip:** a reusable decision trigger or practical shortcut.
- **Warning:** a realistic footgun and how to avoid it.
- **Failure:** exact compiler/runtime/test symptom, cause, and one clear fix.
- **Question:** the exercise.
- **Solution:** collapsed when the renderer supports it.

Do not add a callout for ordinary narration.

## Final lesson check

- The lesson answers one dominant question and matches its execution card.
- Prerequisites and file lists match the actual delta.
- Every stage has evidence and states the expected result before execution.
- Working code matches the verified reference state and complete files contain
  no members from later lessons.
- Version-sensitive claims have nearby primary citations.
- Educational sources shaped pedagogy without becoming unverified technical
  authority or copied prose.
- The running example and terminology remain consistent.
- The exercise occurs after the completed pattern and has a checkable answer.
- Deliberate gaps point forward without explaining drafting history.
- Summary aids retrieval; conclusion synthesizes instead of repeating it.
- Navigation, internal links, headings, callouts, and code fences render in the
  target publication system.

