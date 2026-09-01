---
name: wrap-up
description: >
  Owns the session handoff lifecycle: the end-of-session ritual that captures
  completed work, pending tasks, and learnings into .claude/handoff.md, and
  the session-start protocol that loads it back. Triggers on: /wrap-up,
  "wrap up", "done for today", "that's all", "end session", "signing off",
  "handoff" — and at session start: "start session", "session start", "load
  handoff", "pick up where we left off", "what were we working on".
---

# /wrap-up

**Project-scoped skill** — assumes this repository's own handoff format and
conventions; travels with the rest of `.claude/`, not standalone.

## What

The session continuity ritual. Sessions are ephemeral; knowledge is
permanent. Wrap-up bridges sessions in both directions:

- **Session END** — capture exactly three things: what was DONE, what is
  PENDING, and what was LEARNED. Write them to `.claude/handoff.md`.
- **Session START** — load the handoff and present a resume summary so no
  session starts blind.

This project doesn't run a separate instinct/confidence tracking system —
Claude Code's built-in auto memory already does the "remember a correction
or a non-obvious discovery" job (see the **Learning Extraction** step
below). Wrap-up's job is narrower: the handoff file, which is about *where
you left off*, not *what you learned in general*.

## When

- End of a working session — "done for today", "that's all", "signing off".
- Before switching projects or after a major milestone.
- Implicit endings — "thanks" after completed tasks, "good enough for now":
  offer the handoff, don't just say goodbye.
- Start of a session — "start session", "load handoff", "what were we
  working on".
- For a mid-session save without ending, use `/checkpoint` instead.

## How

### Session End

1. **Review the session** — from git status/diff and the conversation:
   files touched, tasks completed vs. unfinished, decisions made and why.
2. **Check uncommitted changes** — if any exist, offer to commit before
   wrapping.
3. **Write the handoff** — `.claude/handoff.md`, using the format below.
   Single file, always overwritten — only the current state matters. If the
   existing handoff has pending tasks you didn't touch this session, ask
   before overwriting: merge, overwrite, or skip.
4. **Extract learnings** — if the user corrected something or a non-obvious
   discovery surfaced (see checklist below), say so and let Claude Code's
   auto memory capture it; don't duplicate it by hand into the handoff file
   as if it were project state.
5. **Confirm** — summarize the handoff for the user.

### Handoff File Format (`.claude/handoff.md`)

Write it for a stranger with zero context — file paths, rationale, specific
next steps. "Continue the refactor" is useless; "Refactor `EnemyBase.Die()`
to add an `OnDeath()` virtual hook — see `refactor` skill for the
convention" is actionable.

```markdown
# Session Handoff

> Generated: 2026-08-03 | Branch: main

## Completed
- [x] Extracted a `Die()` cleanup step in EnemyBase (scenes/Enemies/EnemyBase/EnemyBase.cs)
- [x] Fixed the Fix64/lockstep contradiction in .claude/rules/doctrine.md

## Pending
- [ ] Confirm Squid and Devourer both still call the shared Die() hook correctly

## Context
- Branch: main | Last commit: "Refactor: extract Die() cleanup into a virtual hook"
- Uncommitted changes: no
```

### Learning Extraction Checklist

1. Did the user correct anything? → let auto memory capture it.
2. Did we discover something non-obvious about this codebase or Godot 4.6
   specifically? → note it in conversation so auto memory can pick it up.
3. Did we decide something with rationale that isn't already an ADR? →
   consider whether it belongs in `.claude/knowledge/decisions/` instead of
   only the handoff.
4. Did a tool/approach fail? → note it so the next session doesn't retry it.

Good learning: "the Break() retrograde-turn logic in Player.cs goes unstable
below a certain velocity because Vector2.Angle() is noisy near zero length"
(specific, non-obvious, durable). Bad learning: "worked on the Player
script" (vague, not durable knowledge).

### Session Start

1. **Load context** — read `.claude/handoff.md` (pending work) if it
   exists. Missing is fine: note it and start clean, never block on it.
2. **Detect the project** — find `project.godot` and `GodotWildJam-96.sln`
   (current dir, then parents) so a build is one command away.
3. **Present a resume summary** — last session's work, pending tasks,
   detected project. Then ask what to work on — never auto-start pending
   work.

## Example

```
User: /wrap-up

Claude: Wrapping up session...

## Session Summary
- Extracted a Die() cleanup step in EnemyBase
- Fixed the Fix64/lockstep contradiction in .claude/rules/doctrine.md

## Pending
- [ ] Confirm Squid and Devourer both still call it correctly

Written to .claude/handoff.md. See you next session.
```

## Related

- `/checkpoint` — mid-session save (commit + brief note) without ending the
  session.
