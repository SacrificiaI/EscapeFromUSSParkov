# ADR-005: No multiplayer/determinism doctrine adopted from the parent Pulang Damit standing instructions

## Status

Accepted.

## Context

`Programming/Godot/Projects/CLAUDE.md` (the standing instructions for the
parent `Godot/Projects/` folder, which also holds Project Pulang Damit) sets
architecture doctrine for that primary project: host-authoritative/
server-authoritative networking over Steam Datagram Relay, a mandatory
sim-presentation split, RPC-validated player intent, and Fix64/lockstep
deferred to a named Phase 10. That same file explicitly scopes side projects
— GodotWildJam96 included — as "separate games treated on their own terms,"
but doesn't spell out what that exemption covers, which leaves room to
reflexively import Pulang Damit's networking assumptions into a project that
never asked for them.

GodotWildJam96 (Solar Defense) has none of the premises that doctrine exists
to solve: it is single-player, has no RPCs, no host/client split, and no
determinism requirement (confirmed by grep — zero `Multiplayer`/`RPC`/
`ENetMultiplayerPeer` hits anywhere in `GodotWildJam96.Game`).

## Decision

**Do not adopt Pulang Damit's networking or determinism doctrine for
GodotWildJam96.** No authority model, no RPC validation, no Steam Datagram
Relay, no Fix64/lockstep — there is no network boundary here to defend and
no cross-machine state to desync. This project's own
[`.claude/rules/doctrine.md`](../../rules/doctrine.md) states the same
rejection as a general rule ("Advanced multiplayer/determinism doctrine is
opt-in, not baseline"); this ADR is the decision record for why that rule
applies here specifically, not a restatement of it.

**Do keep applying everything that isn't networking-specific** from the
parent instructions and from the user's global doctrine: the three-level
performance model, sim-view separation as earned-not-default, and the
communication/code-review standards — those apply project-wide regardless
of a project's networking model.

## Consequences

### Positive

- No architecture imported that has no corresponding requirement here (no
  RPCs to validate, no authority boundary to design, no lockstep to keep
  deterministic).
- `float` throughout `Player.cs`/`Sun.cs`/etc. is correct as written, not a
  latent bug — determinism only matters with cross-machine state to
  reconcile, which this project doesn't have.

### Negative

- If this project ever grows a networked mode, Pulang Damit's doctrine
  becomes directly relevant and should be revisited — it wasn't rejected
  because it's wrong, only because it doesn't apply yet.

### Mitigations

- `CLAUDE.md`'s "Out of scope" section and
  [`.claude/agents/godot-advisor.md`](../../agents/godot-advisor.md)'s
  "Scope boundary" both point back here so a future session doesn't
  reflexively pull in the parent folder's multiplayer doctrine for this
  project.
