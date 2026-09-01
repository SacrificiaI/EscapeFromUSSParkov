---
description: >
  Priority order for resolving trade-offs when writing or refactoring code —
  what wins when two concerns pull in different directions.
---

# Priority Order

When a change could be made multiple ways and the "right" approach isn't
obvious, resolve it using this order. A higher item wins when it conflicts
with a lower one.

1. **Correctness** — the code does what it's supposed to do, including edge
   cases (does the hitbox actually disable, does the timer actually reset).
2. **Simplicity** — the most straightforward implementation that still
   satisfies correctness. Fewer moving parts over clever abstractions.
3. **Documented intent, if the project calls for it** — a learning-focused
   project may deliberately keep comments that explain *why*, name a Game
   Programming Pattern in use, or point at planned future work, because the
   comments are as much the point of the exercise as the code. If a
   project's own `CLAUDE.md` establishes a comment convention like this,
   don't strip it during cleanup — preserving pedagogical scaffolding wins
   over a terser diff. A project with no such convention has no reason to
   invent one here.
4. **Readability & maintainability** — a future reader (including future-you)
   understands it without archaeology.
5. **Performance** — split into two different bars, not one:
   - Free habits that cost nothing extra to write correctly the first time
     (a plain loop instead of LINQ in repeated code, `StringBuilder` for
     multi-piece strings, one clear data owner per field) are applied by
     default, not gated on profiling — refactoring them in later is pure
     avoidable work.
   - Complexity-adding tools (`Span<T>`, `ArrayPool<T>`, object pooling,
     `ref`/`in`, `readonly struct`) are gated on the target hardware
     (integrated GPU, 8 GB RAM — see [doctrine.md](doctrine.md)) or an
     actual profiling result. Don't guess with these.

## How to apply

- **DON'T** reach for `Span<T>`, `ArrayPool<T>`, object pooling, or `ref`/`in`
  parameters unless correctness and simplicity are already settled and the
  code is on a measured or obviously-hot path (`_Process`, `_PhysicsProcess`,
  per-tick loops). See [performance.md](performance.md) for specifics.
- **DO** apply the free habits above by default, even outside a hot path,
  when they cost nothing extra — see
  [performance.md](performance.md#free-habits--apply-by-default-not-just-on-hot-paths)
  for the full list and the line between "free" and "complexity-adding."
- **DON'T** sacrifice simplicity for a "smarter-looking" abstraction that
  doesn't change behavior — a state machine for two booleans, an interface
  for a type with one implementation.
- **DO** default to the plainest working solution that still reads well as a
  learning artifact, then move down the list only if a concrete need (a real
  bug, a real maintainability pain point, a measured perf problem) forces it.
- When two rule files disagree in a specific case, this ordering is the
  tiebreaker — e.g. a correctness fix always wins over a performance
  shortcut, and pedagogical clarity wins over shaving a few lines.
