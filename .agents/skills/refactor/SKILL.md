---
name: refactor
description: >
  Refactor Godot 4.7 C# code in this project toward clearer node ownership,
  simpler event wiring, and the project's established conventions, with the
  smallest justified change. Use when asked to clean up, restructure, or
  simplify a script, scene script relationship, or signal/event flow in
  Solar Defense (`GodotWildJam-96`).
argument-hint: 'Target file, symbol, or refactor goal'
user-invocable: true
---

# Refactor

**Project-scoped skill** — assumes this repository's own rule files and
conventions; travels with the rest of `.Codex/`, not standalone.

Use this skill when the task is to move a piece of this codebase toward its
own established conventions with the smallest justified change — not to
introduce new architecture. This project is small (25 files) and
single-player; most refactors here are about consistency and clarity, not
scale.

## Source of Truth

- [.Codex/rules/priorities.md](../../rules/priorities.md) — the tiebreaker
  order (correctness, simplicity, pedagogical clarity, readability,
  performance).
- [.Codex/rules/doctrine.md](../../rules/doctrine.md) — what performance
  doctrine applies, and what explicitly doesn't (no multiplayer/Fix64/
  lockstep concerns in this project).
- [.Codex/rules/godot-csharp-conventions.md](../../rules/godot-csharp-conventions.md)
  — the actual patterns already in use: `[Export]`-only node refs, C#
  `event Action` over `[Signal]`, subscribe-in-`_Ready`/unsubscribe-in-
  `_ExitTree`, naming, `sealed` by default.
- [.Codex/knowledge/gaming-patterns-index.md](../../knowledge/gaming-patterns-index.md)
  — which Gaming Pattern already fits, before reaching for a new one.

## What Good Refactoring Means Here

- Brings a script in line with this project's own established conventions
  (an inline `GetNode<T>()` call should become an `[Export]` field; a
  `[Signal]` should become a `SignalHub` event if it's project-wide, or stay
  as-is if it's a Godot built-in node signal).
- Untangles a script doing more than one job into smaller focused methods —
  the existing `Player.cs` split (`Fall`, `MoveSideways`, `ApplyHurtJump`) is
  the model to match.
- Fixes a signal subscribe/unsubscribe mismatch (missing `-=` in
  `_ExitTree`, or an unsubscribe that will throw because the handler
  self-unsubscribes elsewhere — see the three named exceptions in
  `godot-csharp-conventions.md`).
- Removes dead code or an unused export/field your own change made unused.

A bad refactor here is code motion without a real gain, introducing a
pattern this project hasn't earned (object pooling, a hand-rolled FSM where
`AnimationTree` already does the job, an interface for a type with one
implementation), or stripping the `PATTERN:`/`FORWARD POINTER` comments that
are this project's whole point.

## Refactor Procedure

1. **Find the narrowest concrete anchor.** Start from a named file, method,
   or a described problem. Don't scan the whole project first.
2. **Read enough of the file and its scene (`.tscn`) to understand the
   real problem.** Categorize it: correctness, convention drift (doesn't
   match `godot-csharp-conventions.md`), signal lifecycle bug, or plain
   readability.
3. **Choose the smallest target state that fixes it**, matching an existing
   pattern in this codebase rather than inventing a new one.
4. **Make the change. Don't refactor the neighborhood** — preserve
   unrelated code, comments, and formatting exactly as they are.
5. **Validate immediately**: `dotnet build GodotWildJam-96.sln` from the
   project root. There is no test suite; a clean build plus a manual check
   of the changed behavior is the bar.
6. **Report what changed and why**, citing the convention or ADR that
   justified it.

## Output Expectations

State: what was wrong, what changed, which rule/ADR/pattern justified it,
that the build passed, and anything intentionally left alone.
