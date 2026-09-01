---
name: pattern-check
description: >
  Map a described design problem to the least sophisticated Game Programming
  Pattern that solves it, using this project's own pattern index and what it
  already demonstrates. Use when deciding how to structure new gameplay
  code, whether a pattern is warranted, or which existing pattern in this
  codebase a new system should match.
argument-hint: 'The design problem or system being added'
user-invocable: true
---

# Pattern Check

**Project-scoped skill** — assumes this project's own rule files and
pattern index; travels with the rest of `.claude/`, not standalone.

GodotPrompter's skills know general Godot architecture. They don't know
which Game Programming Patterns this specific 26-file codebase already
uses, or which ones it has deliberately not earned yet at its current
scale. This skill is for that gap.

## Procedure

1. **State the actual problem in one sentence** — not "what pattern should I
   use" but the concrete thing that's hard: "two unrelated scripts need to
   react to the boss dying," "this enemy has four mutually exclusive
   behaviors and it's turning into a boolean swamp," "I need to spawn
   bullets without the shooter knowing who consumes them."

2. **Check [gaming-patterns-index.md](../../knowledge/gaming-patterns-index.md)
   first** for the problem → pattern map (each row a self-contained
   one-line definition), then **check [CLAUDE.md](../../../CLAUDE.md)'s
   Architecture section** for what this project already demonstrates. If an
   existing instance fits (Observer via `SignalHub`, Component via
   `HitBox`/`Shooter`/`Lifetime`, Template Method via `EnemyBase`, State via
   `AnimationTree`), match its shape rather than inventing a second style
   for the same problem.

3. **If no existing instance fits**, the index's one-line definition is
   usually enough to confirm the pattern fits the stated problem. The free
   chapter linked from that row (gameprogrammingpatterns.com) covers the
   fuller "When to Use It" / tradeoffs discussion if more depth is needed —
   optional reading, not required to finish this check.

4. **Recommend the least sophisticated pattern that solves the stated
   problem**, per [doctrine.md](../../rules/doctrine.md). This project is
   single-player at small scale: Object Pool, Spatial Partition, and Double
   Buffer are real, well-documented patterns with no earned use here yet.
   Don't recommend them speculatively — say so explicitly if the problem
   doesn't need them.

5. **State the tradeoff, not just the answer.** Every pattern trades
   something for its benefit — an event bus adds a layer of indirection; a
   state machine adds files for two states that a plain bool would cover.
   Name the specific cost for the specific problem being solved, not a
   generic warning.

## Output

- The one-sentence problem restated.
- The recommended pattern, with the existing project instance to match if
  one exists, or a note that it's new to this codebase.
- One sentence on the tradeoff being accepted.
- If no pattern is warranted at all (the plain code is simpler and the
  problem doesn't justify indirection), say that instead.