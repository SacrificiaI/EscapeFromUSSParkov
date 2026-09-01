---
name: checkpoint
description: >
  Mid-session save point: create a descriptive git commit and a brief handoff
  note, then keep working. Use before a risky refactor, when switching
  tasks, or to bank progress without ending the session. Triggers on:
  /checkpoint, "checkpoint", "save progress", "commit and handoff", "save
  state", "pause here", "before a risky change". For the full end-of-session
  ritual, use /wrap-up instead.
---

# /checkpoint

**Project-scoped skill** — assumes this repository's own conventions
(commit-message prefixes, the `.claude/handoff.md` format shared with
`/wrap-up`); travels with the rest of `.claude/`, not standalone.

## What

A quick mid-session save that banks the known-good state in two moves:

1. **Descriptive git commit** — stage relevant changes and commit with a
   message that summarizes the work.
2. **Brief handoff note** — write `.claude/handoff.md` so a resumed session
   (or you, after a botched refactor) knows exactly where things stand.

Checkpoint is the mid-session save; `/wrap-up` is the end-of-session ritual.
Checkpoint commits and jots a note, then keeps working.

## When

- Before a risky refactor or destructive change — checkpoint the known-good
  state first (e.g. before touching `EnemyBase` and its three subclasses at
  once).
- When switching to a different task or feature mid-session.
- After completing a logical unit of work — bank it.
- User says "checkpoint", "save progress", "save state", "pause here".
- If the session is actually ending, use `/wrap-up` instead.

## How

1. **Assess state** — `git status` and `git diff` to see staged and
   unstaged changes.
2. **Stage changes** — stage specific files by name; never `git add -A`.
   Exclude `bin/`, `obj/`, `.godot/` if they somehow show as untracked, and
   anything that isn't a real content change.
3. **Commit** — descriptive message following the repo's existing style
   (`Feat:`, `Fix:`, `Chore:` prefixes — see recent `git log`). New commit,
   never amend. If there's nothing to commit, skip to the handoff.
4. **Write the handoff note** — `.claude/handoff.md`, same format as
   `/wrap-up` but abbreviated: Completed, Pending (with specific next
   steps), and Context (branch + commit hash). One file, always overwritten.
5. **Confirm** — report the commit hash and the handoff summary, then
   resume work.

### Safety Rules

- Never push, force-push, or touch remote branches.
- Never amend — a checkpoint is a new commit you can return to.
- No changes to commit? Update the handoff only.

## Example

```
User: /checkpoint — about to split EnemyBase's Die() logic

Claude: 2 files modified.

  Commit a1b2c3d: "Refactor: extract Die() cleanup into a virtual hook"
  - scenes/Enemies/EnemyBase/EnemyBase.cs
  - scenes/Enemies/Snail/Snail.cs

  Handoff updated (.claude/handoff.md):
  ## Completed
  - [x] Extracted OnDeath() virtual hook in EnemyBase
  ## Pending
  - [ ] Update Eagle and Frog to override the new hook if needed
  ## Context
  - Branch: main | Checkpoint: a1b2c3d

  Checkpoint saved — safe to proceed.
```

## Related

- `/wrap-up` — end-of-session handoff, plus the session-start resume
  protocol.
- `/refactor` — the skill this usually brackets.
