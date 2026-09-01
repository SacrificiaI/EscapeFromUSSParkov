---
description: >
  How to scale architecture and performance doctrine to a project's actual
  size and requirements, and which advanced doctrine (multiplayer,
  determinism, spatial partitioning at scale) only applies once a project
  actually has the need that justifies it.
---

# Architecture & Performance Doctrine

## Performance: the three-level model

1. **Architecture** — plan for known scale. A project's scale is whatever
   it actually is: single-player vs. multiplayer, a handful of entities on
   screen vs. hundreds, networked vs. not. There's no "known future scale"
   to architect ahead of until a project actually states one — planning for
   a scale the project doesn't have yet is speculative complexity, not
   foresight.
2. **Implementation** — write the clear version first, then measure.
3. **Micro-optimization** — only after profiling identifies a real hot
   path.

Don't recommend object pools, `MultiMesh`, struct arrays, `Span<T>`,
threading, or `ArrayPool<T>` as ritual on a project that has never profiled
a bottleneck. See [performance.md](performance.md) for the hot-path rules
that apply regardless of profiling status (allocation discipline in
`_Process`/`_PhysicsProcess`) — those are worth following by default
because they cost nothing extra to write correctly the first time, not
because they've been measured as necessary.

## Advanced multiplayer/determinism doctrine is opt-in, not baseline

Lockstep simulation, Fix64/fixed-point determinism, host authority and RPC
validation, a dedicated relay transport, and spatial partitioning tuned for
multiplayer-scale proximity checks are all real, well-documented techniques
— for a project that actually has networking and a determinism requirement.
A single-player (or non-deterministic multiplayer) project has none of the
premises these techniques exist to solve:

- **Lockstep / exterior-interior simulation splits** solve keeping multiple
  independent machines' simulations in sync. Nothing to synchronize with
  one machine.
- **Fixed-point math (Fix64 or similar) for determinism** solves floating
  point producing slightly different results on different hardware/compiler
  combinations, which only matters when multiple machines must derive
  identical results from the same inputs. `float` is fine anywhere there's
  no cross-machine state to desync.
- **RPCs, host authority, server-side validation** solve one machine's
  input being untrustworthy to every other machine. There's no network
  boundary to defend without a network.
- **Relay/transport-layer concerns** solve connecting untrusted machines
  across the internet. Not applicable with no network layer at all.
- **Spatial partitioning for multiplayer-scale proximity checks** solves an
  O(n²) proximity query becoming a frame-budget problem at a scale
  (hundreds of networked entities) most projects never reach.

If a codebase's own doctrine documents this style of architecture for a
*different* project (a sibling multiplayer project sharing the same
`.claude/` kit, for example), don't import that doctrine into a project
that hasn't stated the same networking/determinism requirements — name the
mismatch and stay with the doctrine that matches the actual project.

## Sim-View separation: adopted project-wide

> [!IMPORTANT]
> **This project has adopted sim-view separation.** The general guidance
> below — that the split is earned, not default — is why it was *not* the
> original architecture, and is still the right default advice for a new
> project. Solar Defense migrated anyway, as a deliberate owner decision
> recorded in
> [ADR-009](../knowledge/decisions/009-sim-view-separation.md). Don't cite
> the paragraphs below to argue new gameplay logic should go back into a
> Node script here: in *this* codebase, gameplay rules go in
> `GodotWildJam96.Sim` and Node scripts are bridges.

A full sim-view-bridge architecture — a pure-C# simulation layer, a bridge
node, a fixed-tick loop independent of Godot's own `_Process`/
`_PhysicsProcess` — is architecture earned by needing its actual benefits:
threading, unit-testing without booting the engine, or networking. A
project with none of those needs is exactly the case where it doesn't apply
yet — put logic in Node scripts instead of pre-building a simulation layer
nothing yet requires.

Deliberately practicing the sim/view boundary as a skill, distinct from *the
project needing it*, is a legitimate reason to use one small subsystem as
low-risk practice ground on a learning-focused project — but scope it to
one already-incomplete subsystem rather than retrofitting a working game,
and don't reach for a DI/state-machine package to do it: the sim/view
boundary is a hand-written architectural discipline, independent of any
library.

Note what this project did **not** adopt along with the split: there is
still no fixed-tick simulation clock. Each bridge drives its simulation
object from `_PhysicsProcess`/`_Process` at Godot's own cadence. A separate
tick loop is a distinct decision that needs its own reason (replay,
rollback, networking) — sim-view separation does not imply it.

## Pattern guidance

See [gaming-patterns-index.md](../knowledge/gaming-patterns-index.md) for
the full problem → pattern map. Map a design problem to the least
sophisticated pattern that solves it — Object Pool, Spatial Partition, and
Double Buffer are patterns most small or single-player projects won't earn
at their actual scale; don't reach for them speculatively.