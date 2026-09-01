---
name: lesson-maker
description: >
  Add entries to a Godot learning project's 00Lessons.md and 01Useful
  Patterns.md files in the personal tutorial vault, matching the series'
  established format, without duplicating anything already recorded
  anywhere in the Learning_01–Learning_0N series. Use after finishing a
  debugging session, a feature, or a refactor pass on one of these
  projects, when something learned is worth recording — or when asked to
  review recent changes for lesson material.
argument-hint: 'What was learned/built, or "review recent changes" to derive it from the current diff'
user-invocable: true
---

# /lesson-maker

**Personal, cross-project skill** — a third kind, distinct from the two this
repository's `skill-authoring.md` names. It hardcodes this user's personal
Obsidian vault path and the `Learning_0N_*` project series as its operating
domain, because that domain *is* the point of the skill — a fully portable
version would be pointless. But it earns the same self-containment a
portable skill gets: it does not depend on the hosting repo's own
`.Codex/rules/` or `.Codex/knowledge/`, and it works identically no
matter which `Learning_0N` project's repo it's invoked from. Everything it
needs is inlined below or detected from disk at run time.

## Scope

Writes to exactly two files per project: `00Lessons.md` and `01Useful
Patterns.md`. Never touches `Anki Lessons.md` or any other per-project note
— those are a different artifact with a different job. Never edits code.
Never retrofits an older project's file to add a forward-pointing
cross-reference (the series' `see-also` chain links backward only, one
project to the one immediately before it — leave that direction alone).

## 0. Locate the series and the target project

Vault root: `C:\Users\jocvi\Documents\Collection-of-Folders\Programming\GameDev\GameDev\JP-Programming\Godot\`.
Override if the caller states a different path.

Series folders are every `Learning_0N_*` directory under that root, ordered
by the leading number — glob for them fresh each run rather than trusting a
hardcoded list, since the series grows. As of this writing it runs
`Learning_01_PoopGame` → `Learning_02_TappyPlane` →
`Learning_03_Angry-Animals` → `Learning_04_Memory-Madness` →
`Learning_05_FoxyAntics`; treat any higher-numbered folder found on disk as
the newest entry, not an anomaly.

Map the invoking repo to its series folder by matching the C# assembly/
root-namespace name (read from the `.csproj`) against the `project:`
frontmatter field in each series folder's `00Lessons.md`. The mapping so far:
`PoopCatcher01` → 01, `TappyPlane01` → 02, `AngryAnimals01` → 03,
`MemoryMadness01` → 04, `FoxyAntics01` → 05. If the invoking repo's assembly
name matches nothing in the series, ask which folder to target rather than
guessing or creating a new one unprompted.

## 1. Read for coverage, not just the target project

1. Read the target project's own `00Lessons.md` and `01Useful Patterns.md`
   in full — every file in the series so far is under 700 lines, cheap to
   read whole.
2. Read at minimum the **Table of Contents** of every *other* project's
   both files. This is the check that catches the high-value case: a
   pattern already established earlier in the series that the current
   project's code has now reintroduced correctly, or — worse and more
   interesting — gotten wrong. A regressed idiom against a project's own
   prior, working code is a better lesson than a freshly-invented one.

## 2. Gather the source material

Either the caller states directly what was learned/built, or the caller
says to review recent changes — in which case read `git log`/`git diff` in
the current project, then **read every file the diff touches, in full**,
before drafting anything. Never reconstruct a code snippet from memory of
"how this usually looks" — every fenced code block in the final entry must
be copied from a file actually opened during this run.

## 3. Classify: Lesson, Pattern, both, or neither

- **Lesson** (`00Lessons.md`) — a narrative entry: something broke, was
  surprising, or took real debugging. Keep it concrete: the actual symptom
  (error text, wrong behavior), the actual cause, the actual fix. Don't
  soften a real gotcha into generic advice — the specific failure mode is
  the entire value of the entry.
- **Pattern** (`01Useful Patterns.md`) — a reusable recipe: terse "Use
  when," a minimal snippet, no narrative windup. If one piece of work
  produced both a war story and a reusable recipe, write both entries,
  cross-linked with a wikilink.
- **Neither** — a routine content change with no generalizable insight
  (placing more enemies in a level, tuning a number). Say so and stop.
  Don't manufacture an entry to have something to show.

## 4. Check for overlap before drafting anything

In order:

a. **Exact or near-duplicate already in this project's own file** → skip
   entirely, or if the new material genuinely extends the existing entry,
   rewrite that entry's snippet to the current, complete shape in place —
   don't stack a second snippet on top as a diff, and don't add a new
   heading for the same idea.
b. **Already established earlier in the series, absent from this
   project's file** → still worth its own entry — each project's file is
   deliberately self-contained, not a merged index — but name the earlier
   project it first appeared in, in one sentence.
c. **This project's current code contradicts something the series already
   got right earlier** → the highest-value case. Name the earlier project
   and what it did correctly, e.g. *"TappyPlane01's `01Useful Patterns.md`
   already has this idiom right (`if (!ResourceLoader.Exists(...)) return;`)
   — this project inverted it."* This is a lesson even when the underlying
   bug has already been fixed in code; the entry documents the trap, not
   the fix.

Every project's intro paragraph since Learning_03 states the house rule
this step exists to enforce, verbatim: *"Only lessons that are genuinely
new or reinforced here are recorded."* Hold new material to that bar.

## 5. Outline first, always

Before writing one full entry, produce: the proposed heading, a one-line
description, the target file, and the result of step 4's overlap check.
Wait for the caller to confirm before writing anything — the TOC edit,
callouts, and snippet formatting are the expensive part, and step 4 alone
can determine there's nothing worth adding.

## 6. Write the full entry, on confirmation

Match the format every file in the series already uses — don't invent a
new shape.

**Frontmatter** — `tags: [godot, csharp, reference, learning]` (Lessons) or
`[godot, csharp, reference, patterns]` (Patterns); `project: <AssemblyName>`;
`see-also:` listing the file's own sibling first, then the immediately
prior project's same-type file (Lessons↔Lessons, Patterns↔Patterns). Only
touch frontmatter when creating a project's first-ever entry in a file —
an existing file's frontmatter is already correct; leave it alone.

**Heading** — a plain descriptive or imperative sentence; backtick any code
identifier it names (`` `[Export]` Beats `GetNode` Every Time ``).

**Lesson entry shape:**

```markdown
## Heading Text

Narrative paragraph — what happened, why, the mechanism. Include the real
error text verbatim if there was one.

From project code: `File.cs`

\`\`\`csharp
// real code, copied from a file read this run
\`\`\`

> [!TIP]
> **In practice:** the generalization — where else this trap/insight applies.
```

Add `> [!WARNING]` only when there's a real footgun worth calling out
separately from the main narrative.

**Pattern entry shape:**

```markdown
## Heading Text

> [!TIP]
> **Use when:** one concrete trigger condition, one line.

\`\`\`csharp
// File.cs
...real snippet...
\`\`\`
```

No narrative windup — the callout states the trigger, the snippet *is* the
teaching. A short trailing paragraph is fine if the snippet alone needs one
line of context, not more.

**File-reference convention drifted slightly across the series** — some
projects' Patterns entries put the file name in a `// File.cs` comment
inside the fence, others in a leading "From project code:" line matching
the Lessons style. Confirm which the specific target file already uses on
its existing entries and match that file's own convention, not the other
file's.

**TOC** — append the new heading to the file's own Table of Contents. If
the target file already uses a sub-grouping (FoxyAntics01's two files use
`### Added in the second refactor pass`), and this addition is part of a
similarly distinct wave of work, add a new sub-heading in that same style;
don't invent that grouping for a project whose file has never used it.

**Cross-series reference** — when step 4b or 4c applies, add one plain
prose sentence naming the earlier project and pointing at its entry. The
series' `see-also` frontmatter chain doesn't cover this (it only links
whole files, one hop backward) — this is inline prose inside the entry
body, same as the existing supersession note in FoxyAntics01's
`00Lessons.md` ("This supersedes the workaround in [[#...]]").

## 7. One deliberate format improvement, opt-in only

When the source code being documented is an explicit placeholder — its own
header comment says so, matching this project's `PLACEHOLDER STATUS`
convention — add a one-line `> [!INFO]` scope callout: what's deferred, and
what will eventually replace it. Use this only when the code itself already
declares placeholder status; don't invent a deferral to make an entry look
more forward-looking than the code actually is.

## 8. Report

After writing: which file(s) changed, which heading(s) were added or
extended in place, and any earlier-series cross-reference found in step 4.
