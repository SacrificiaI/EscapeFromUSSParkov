---
paths:
  - ".claude/skills/**/*.md"
description: >
  Standards every skill file in this project must meet: self-contained (no
  dependency on a file, wiki, or memory system outside the skill itself) and
  free of conversation/process narration. Learned the hard way authoring
  godot-init — first draft cited files a teammate copying the skill
  wouldn't have, and read like a transcript of how it got designed instead
  of a finished document.
---

# Skill Authoring

## Self-contained — but know which kind of self-contained

Two different kinds of skill live in this repository, and they carry
different portability requirements. State which kind a new skill is near
the top of its file, since the rest of this section depends on it.

- **Portable, project-agnostic skills** (`godot-init` is the example) are
  meant to work for someone who has *only that one file* — a friend on an
  unrelated project, a teammate who copy-pasted just the one skill folder.
  These get the full standard below: no dependency on any file outside
  themselves, inline everything that matters.
- **Project-scoped tools** (`refactor`, `checkpoint`, `pattern-check`,
  `wrap-up`) are deliberately tied to this project's own conventions and
  reference this repository's own `.claude/rules/` and `.claude/knowledge/`
  files on purpose — that's not a violation, because the whole `.claude/`
  directory is expected to travel together when this project's skills get
  copied elsewhere (see project memory: copy-paste, not a plugin, is the
  sharing mechanism here). Linking `../../rules/priorities.md` from inside
  `.claude/skills/refactor/` is fine specifically because both files move
  as one unit.

For a **portable** skill:

- **No relative markdown links out of the skill's own file** (`../../../CLAUDE.md`,
  `../../rules/doctrine.md`, and similar). If the content matters, inline it.
  If it's only a citation, restate the fact it supports and drop the link.
- **No `[[wikilinks]]`** to an Obsidian vault or any note collection outside
  this repository. Same fix: inline what the link was standing in for.
- **No references to "project memory," a prior session, or any other
  Claude-Code-local mechanism.** Those exist on this machine, for this
  user, in this session — not for whoever else ends up running the skill.
- Internal cross-references **within the same skill file** (`see item 12`,
  `per Status above`) are fine and expected — the reader has that part too.

For a **project-scoped** skill, links to this project's own rule and
knowledge files are the expected pattern, not an exception to excuse.

Before calling a skill file done, grep it for `](../`, `[[`, and words like
*memory*, *conversation*, *this project's* — each hit is either a fact that
needs inlining or a sentence that needs rewriting to stand on its own.

## Voice: no narration of how the file came to be

Adapted from this same standard already enforced for the vault's tutorial
docs — a skill reads as a finished, authoritative document, not a record of
the conversation that produced it.

- **No deference.** Never mention who asked for this or reference a prior
  conversation — "as discussed," "per your request," "earlier," "just
  moved," "turns out to exist." Write with the authority of the subject
  matter, not as a reply to somebody.
- **No hedging.** Cut "might," "could potentially," "it's worth noting,"
  "arguably." State the rule directly.
- **No process narration.** Don't describe how the content was derived
  ("raised early and dropped by accident," "reviewed and agreed in
  conversation"). If something is a genuinely unresolved design question,
  say so as a standing fact of the document ("open question: X") — not as
  a status update on a discussion.
- **No history in the file.** Don't write "this replaces X" or "this used
  to say Y." A skill file describes what's true now.

**Carve-out — this doesn't ban real content.** A genuinely open design
question, a tradeoff between options that are still both live, or a named
edge case the standard behavior doesn't cover are valuable and stay exactly
as they are. The rule bans narrating the file's own authorship, not stating
things the file is honestly still uncertain about.

## Baseline format

Match the existing skills in this repository (`refactor`, `checkpoint`,
`pattern-check`, `wrap-up`, `verify-doc`):

```yaml
---
name: skill-name
description: >
  What it does and when to use it, written so the description alone is
  enough to decide whether this is the right skill for a task.
argument-hint: 'What the caller should pass, if anything'
user-invocable: true
---
```

## Draft status

A skill file that isn't fully built yet — the questionnaire is locked but
the generation logic isn't written, say — states that plainly at the top
and **omits `user-invocable: true`** until it's actually functional. A
half-built skill that shows up in the `/` menu is worse than one that
doesn't show up yet.
