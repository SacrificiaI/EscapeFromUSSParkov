# ADR-009: Project-wide sim-view separation

## Status

Accepted — supersedes the "no sim-view split" position recorded in
[CLAUDE.md](../../../CLAUDE.md) and softens
[doctrine.md](../../rules/doctrine.md)'s "earned, not default" rule for
this project specifically.

## Context

Solar Defense shipped with all gameplay logic inside Godot `Node` scripts.
That was a defensible call and was documented as one: the project is
single-player, short-lived, has no networking, no threading and no replay
requirement, so it had none of the premises that normally pay for a
simulation assembly. [ADR-008](008-refactor-judgment-calls.md) went
further and recorded a deliberate choice to extract `EnergyPool`,
`ChargeMeter` and friends as plain C# collaborators *inside* the Game
assembly rather than routing them through the empty `GodotWildJam96.Sim`
scaffolding.

The project owner subsequently decided to carry out the full migration
anyway, with the trade-off stated plainly at the time: the split's usual
justifications still don't apply here, and the value taken instead is
testability of every gameplay rule plus the boundary being enforced by the
compiler rather than by discipline. This ADR records that the decision was
made with eyes open, so a future reader doesn't mistake the architecture
for drift, or "correct" it back toward ADR-008's narrower position.

## Decision

### 1. Three assemblies, one-way references

`GodotWildJam96.Sim` (no Godot reference) ← `GodotWildJam96.Game` (the
Godot project). `GodotWildJam96.Tests` references **only** Sim.

Cutting the Tests → Game reference is the load-bearing part. It converts
"did a Godot dependency leak into the simulation" from something a reviewer
has to notice into something that fails a build.

### 2. Sim is Godot-*namespace*-free, not just `GD.*`-free

The [refactor roadmap](../refactor-roadmap.md) had established that
`Vector2`/`Mathf` are pure managed types safe to use in a test host, and
only `GD.*` needs avoiding. That remains true. It was nevertheless rejected
here in favour of the stricter line already asserted by
`GodotWildJam96.Sim.csproj` ("Intentionally NO Godot package/reference
here. That's the whole point."): Sim uses `System.Numerics.Vector2` and
converts at the bridge.

The cost is real and should be understood before anyone reverses it:

- Every vector crossing the boundary converts, including inside
  `_PhysicsProcess`. `SimVec` keeps that in one place, but it is a struct
  copy per call rather than zero.
- `System.Numerics` is not a drop-in for Godot's vector API.
  `SimMath` reimplements `FromAngle`, `Angle`, `Normalized`, `DirectionTo`,
  `LimitLength`, `AngleDifference`, `RotateToward` and `Lerp` against Godot
  4.7's actual semantics. **`Normalized()` is the dangerous one**: Godot
  returns `Zero` for a zero-length vector, `Vector2.Normalize` returns
  `NaN`. A naive swap silently produces `NaN` positions for an enemy
  sitting exactly on its target. Two tests pin this.

The benefit taken in exchange is that the boundary needs no judgment call
to police — there is no "but this Godot type is actually fine" conversation
to have, because no Godot type resolves in Sim at all.

### 3. No fixed-tick clock came with it

Each bridge still drives its simulation object from Godot's own
`_PhysicsProcess`/`_Process`. Determinism, replay and rollback are a
separate, larger decision and were explicitly not bundled into this one.

### 4. The bridge holds no second copy of simulation state

Where Godot genuinely owns a value — `CharacterBody2D.Velocity`, which
`MoveAndSlide` rewrites through collision response — the bridge loads it
into the simulation at the top of the tick and writes it back after,
instead of the simulation keeping a copy that drifts. Where the simulation
owns the value, the node exposes it as a pass-through property
(`Player.InLightRadius`, `Player.SiphonUnderway`) rather than a field it
also writes.

## Consequences

### Positive

- Every gameplay rule in the game is now covered by tests that run in
  milliseconds with no editor open: 86 tests, up from 21.
- Several rules that were previously implicit are now named and pinned —
  the owner-dependent sun drain floor, the siphon-start cancel path that
  used to strand `SiphonUnderway`, the post-death enemy hit.
- `Player.cs` and `Sun.cs` are now readable as "what the player sees"
  without the rules interleaved.

### Negative

- The costs in §2 are permanent as long as this choice stands.
- Two `GameConstants` group-name tests were lost when the Tests → Game
  reference was cut. Those strings are a real runtime contract between
  `AddToGroup` and `GetNodesInGroup`, and are now unverified. Restoring
  them would mean either a second test project that references Game, or
  moving the group names into Sim — neither was worth it for two string
  literals, but the gap is real.
- The migration is wider than the project's actual needs, exactly as the
  general doctrine warns. If this codebase is ever cited as an example, it
  should be cited as "a project that adopted this deliberately", not as
  evidence that a small single-player game needs it.

### Mitigations

- If the conversion cost at the bridge ever shows up in a profile, the
  cheapest reversal is adding a `GodotSharp` reference to Sim and deleting
  `SimVec`/`SimMath` — the class boundaries themselves would not move.
