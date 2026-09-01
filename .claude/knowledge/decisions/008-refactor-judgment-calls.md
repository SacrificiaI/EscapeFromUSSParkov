# ADR-008: Three judgment calls from the exemplar-codebase refactor

## Status

Accepted.

## Context

The full 8-phase refactor ([refactor-roadmap.md](../refactor-roadmap.md))
turned Solar Defense from a shipped jam entry into a SOLID/DRY teaching
codebase without changing game feel. Three decisions made along the way
aren't visible from the diff alone — each rejected a more "textbook"-looking
alternative for a reason specific to this codebase's actual scale. Recording
them here so a future reader doesn't "fix" the code back toward the rejected
alternative, thinking the simpler choice was an oversight.

## Decision

### 1. Plain C# collaborators over scene-component child nodes (Phase 5)

`Player.cs`'s decomposition (`EnergyPool`, `ChargeMeter`, `ThrusterAnimator`)
uses plain C# objects Player constructs and owns in `_Ready()`, not child
`Node`s wired via `[Export]` and composed in `Player.tscn`.

Scene composition is the right call when variants need to be assembled
differently in the editor, or when non-programmers need to reconfigure the
composition without touching code
([godot-csharp-conventions.md](../../rules/godot-csharp-conventions.md) on
inheritance vs. composition). None of that applies here: `EnergyPool` and
`ChargeMeter` are pure `Vector2`/`Mathf` math with zero scene-tree
footprint — turning them into nodes would add `.tscn` wiring and a
scene-tree round-trip for no behavior a plain object doesn't already give,
and would cost the one thing that made them worth extracting: unit-testing
them in `GodotWildJam96.Tests` without booting the engine. `ThrusterAnimator`
*is* Godot-coupled (drives four `AnimatedSprite2D`s), but Player already
owns those sprites as `[Export]` fields — routing it through a child node
would mean re-deriving references Player already has, through an extra
scene-tree hop, for a class with exactly one caller and no reuse case.

### 2. `System.Random` injection over an `IRandomSource` interface (Phase 4)

`SpawnPlacement.cs`'s pure placement math takes a plain `System.Random`
parameter. No `IRandomSource`/`IRandomProvider` interface exists.

An interface earns its keep when there's a second implementation or a
polymorphism need — [priorities.md](../../rules/priorities.md) rejects
abstraction without one. `System.Random` is already injectable and already
seedable (`new Random(12345)` for a deterministic test vs. `new Random()`
for real play); wrapping it in an interface with exactly one production
implementation (`System.Random` itself) adds a layer of indirection whose
only purpose would be to make the code *look* more testable, when it already
was. **Dependency inversion does not mean "always add an interface" — it
means depend on an abstraction when a second concrete shape is real, not
hypothetical.**

### 3. The sun-placement distribution stays deliberately unfixed (Phase 4)

`SpawnPlacement.RandomSunPosition` draws radius from `[-5000, 5000]` rather
than `[0, 5000]` and multiplies it straight into the position vector rather
than `sqrt`-correcting it — so placement is uniform in *radius*, not *area*
(suns cluster toward the origin), and a negative radius draw mirrors the
angle instead of pushing the point further out. Both are quirks of the
original jam code, not designed behavior.

Phase 4's characterization tests
(`SpawnPlacementTests` in `GodotWildJam96.Tests`) pin this distribution
exactly as shipped, rather than correcting it. Fixing it would change where
suns land on every run, which changes difficulty — a game-feel decision, not
a refactor. The governing 8-phase refactor plan froze game feel for the
entire pass except three explicitly-named behavior fixes (the Player
no-suns crash, Spawner's 25th-attempt discard, MainSun's re-triggering
scene change), and this distribution quirk was deliberately **not** the
fourth. Whether to correct it is a separate, standalone decision
for whoever owns game balance next — the test suite now makes that decision
safe to make later, by pinning the current behavior so a change shows up as
an intentional, reviewable diff instead of a silent shift.

## Consequences

### Positive

- A future contributor reading `EnergyPool`/`ChargeMeter` as plain classes,
  or `SpawnPlacement` taking a bare `Random`, has a record that these are
  deliberate choices tied to this project's actual scale — not gaps to
  "professionalize."
- The sun-distribution quirk is now a documented, tested, reversible
  decision instead of an undocumented accident someone might "fix" by
  reflex during an unrelated change.

### Negative

- None of these three decisions are revisited automatically if the
  project's scale assumptions change (e.g., if `SpawnPlacement` ever needed
  a second, non-`System.Random` source). This ADR doesn't set a trigger
  condition for revisiting them.

### Mitigations

- If a second concrete need for swappable randomness or swappable placement
  distributions ever appears, that is itself the signal to revisit decision
  2 — the absence of that need today is exactly why it's deferred, not
  ruled out permanently.
