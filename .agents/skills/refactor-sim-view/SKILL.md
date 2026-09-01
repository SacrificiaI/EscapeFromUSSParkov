---
name: refactor-sim-view
description: >
  Migrate a Godot 4.x C# project from mixed Node-script logic into a
  complete sim-view separation: a pure, engine-free simulation assembly plus
  a thin Godot bridge/view layer, with xUnit (or the project's existing test
  framework) tests covering the simulation directly. Use once a project has
  decided to adopt sim-view separation and wants the migration carried out
  end-to-end, system by system, rather than spot-refactored — not to decide
  whether the project should adopt it in the first place.
argument-hint: 'Target system, script, or "whole project"'
user-invocable: true
---

# Refactor: Sim-View Separation

**Portable, project-agnostic skill** — self-contained; works for anyone who
has only this file, on any Godot 4.x C# project.

## Scope

This skill does one thing: take gameplay logic that currently lives inside
Godot `Node` scripts and split it into (a) a pure C# simulation layer with
zero engine dependency, and (b) a thin bridge layer of `Node` scripts that
drive that simulation and render its state. It also stands up or extends
the unit test project that proves the simulation layer never needed the
engine in the first place.

It does not decide *whether* a project should do this. That's a real
architectural cost — a second assembly, a translation layer between input
and sim commands, and discipline about which side owns which piece of
state — and it only pays for itself against a concrete need: unit-testing
gameplay rules without booting the editor, offloading simulation work to a
thread, a networking/replay/rollback requirement that needs deterministic
state transitions, or a deliberate, scoped decision to practice the
sim/view boundary as a skill. A small, short-lived, single-player project
with none of those needs gets nothing from this split except the cost of
maintaining it — if that's the situation, say so and stop instead of
performing the migration anyway. This skill assumes the decision to adopt
sim-view separation has already been made; it exists to make sure that
decision, once made, gets carried out completely rather than half-applied
to one file and abandoned.

## The Target Architecture

Three build boundaries, each an actual project reference graph, not just a
folder convention:

1. **Simulation assembly.** A plain .NET class library (matching the
   Godot project's own `TargetFramework`, e.g. `net8.0`) with **no
   reference to `Godot.NET.Sdk` or `GodotSharp`**. Holds every piece of
   gameplay state and every gameplay rule: entity data, health/resource
   pools, movement and combat math, spawn and win/lose logic, anything a
   unit test should be able to assert on without a running engine.

   Godot's own `Vector2`, `Vector3`, `Vector2I`, `Transform2D`,
   `Transform3D`, `Quaternion`, `Basis`, `Color`, `Rect2`, and `Mathf` are
   pure managed structs and static methods with no P/Invoke — they run
   correctly in a plain `dotnet test` host with no engine attached, and are
   safe to use inside the simulation assembly. Anything reachable only
   through `GodotObject`/`Node`/`Resource`/the static `GD` class requires
   the native engine runtime and must never appear here. If the project
   wants the simulation layer fully independent of the `Godot.*` namespace
   too (for portability to a future engine, or stricter enforcement), use
   `System.Numerics.Vector2`/`Vector3` instead and convert at the bridge
   boundary — state that tradeoff to whoever owns the migration rather than
   choosing it unasked, since it adds a conversion at every bridge call.

2. **Game/View assembly.** The existing Godot project. References the
   simulation assembly. Contains only: `Node` scripts that construct or
   receive simulation objects, translate `Input.*` calls and physics
   signals (`BodyEntered`, `AreaEntered`) into simulation commands, call
   the simulation's tick/command methods, and read simulation state back
   out to drive `Position`, `AnimatedSprite2D.Play(...)`,
   `AudioStreamPlayer.Play()`, UI labels, camera work, and scene
   transitions.

3. **Test assembly.** A plain xUnit project referencing only the
   simulation assembly — no Godot reference, no engine bootstrap. If the
   project already has an established test framework (NUnit, MSTest),
   match it instead of introducing a second one; xUnit is the default only
   because it needs no project-specific justification. This project must
   build and run **without** the Godot editor or `GodotSharp.dll` — that is
   the actual proof the split worked, not an aspiration about it.

**Dependency direction is one-way, enforced by the reference graph itself:**
Game → Sim. The simulation assembly has zero project references back to the
Game assembly. This turns "did a Godot dependency leak into the simulation"
into a build failure the moment someone adds `using Godot;` and calls
`GD.Print` in a file the Sim project can't actually resolve `GodotSharp`
from — not something a reviewer has to notice by reading every diff.

The same rule extends past Godot itself: a platform SDK (Steam, a
save-cloud API, an ads/IAP library, anything with its own native or
network dependency) never appears in the simulation assembly either — wrap
it in the bridge exactly the way the engine itself is wrapped. "No engine
dependency" is really "no dependency the simulation can't own and can't
run without"; Godot is just the first and most obvious instance of that
rule, not the only one.

**Ownership rule: the simulation is the single source of truth for
gameplay state.** A `Node`'s fields are either a direct reference into
simulation state, or a value read fresh from the simulation every frame —
never a second copy the bridge must remember to keep in sync. A `Node`
caching `private int _localHealth` alongside calling
`_entity.TakeDamage(amount)` is the single most common way this migration
silently regresses back into two owners for one fact.

## What Belongs Where

**Simulation layer:** entity state (position, velocity, health, resource
pools, inventories, cooldown/timer values), gameplay rules (damage
resolution, movement integration, the *decision* half of collision
response), win/lose conditions, RNG-driven content decisions (spawn
tables, procedural placement) through an injected `System.Random` — inject
the concrete type; add an interface around it only if a second real
implementation exists, not preemptively.

**View/bridge layer:** node tree structure, `[Export]`-wired child/resource
references, animation and audio playback, camera behavior, input polling,
scene transitions (`ChangeSceneToFile`), anything that requires the
running engine to observe or act on.

**The gray area — collision detection specifically:** the physics engine's
detection (`Area2D`/`CharacterBody2D` signals, raycasts) is the engine's
job and stays in the bridge. The bridge receives the collision event and
converts it into a simulation command (`_entity.TakeDamage(amount)`); the
simulation never queries the physics world directly, and the bridge never
decides the damage amount or whether it applies — that's a rule, and rules
live in the simulation.

## Choosing the Command Surface Shape

Two shapes cover most gameplay systems. Pick per system — a project can run
both at once (a real-time player controller next to a turn-based inventory
menu) — and neither is more "correct" than the other; they fit different
kinds of gameplay.

**Continuous.** A `Tick(float deltaSeconds)` method advancing state every
call, plus direct mutation methods for discrete actions (`TakeDamage`,
`Drain`, `Gain`), and read-only properties for the bridge to poll every
frame. Fits anything already driven by `_PhysicsProcess`/`_Process` —
movement, health pools, cooldown timers, real-time arcade combat. This is
the shape used in the worked example below.

**Discrete, commands-in/events-out.** One entry point —
`IReadOnlyList<TEvent> Execute(TCommand command)` — resolves an entire
action instantly and returns an ordered list of events describing exactly
what happened:

```csharp
public abstract record BattleEvent;
public sealed record UnitMoved(UnitId Unit, Hex From, Hex To) : BattleEvent;
public sealed record UnitAttacked(UnitId Attacker, UnitId Target, int Damage, bool Killed) : BattleEvent;

public interface IBattleCommand { }
public sealed record MoveCommand(UnitId Unit, Hex To) : IBattleCommand;
public sealed record AttackCommand(UnitId Unit, UnitId Target) : IBattleCommand;

public IReadOnlyList<BattleEvent> Execute(IBattleCommand command) => command switch
{
    MoveCommand m   => ResolveMove(m),
    AttackCommand a => ResolveAttack(a),
    _ => throw new ArgumentOutOfRangeException(nameof(command)),
};
```

Fits turn-based systems, anything that needs input locked during animation
playback, and anything that wants replay, an ordered history, or undo. Use
`record` types for both commands and events — immutable and value-equal, so
`Assert.Equal(expectedEvents, actualEvents)` works directly in a test, and
two independently-run simulations can be compared event-for-event.

This shape carries one purity rule with an outsized payoff: **seed the
simulation's `System.Random` once, in its constructor, and never call
anything random-like from outside it.** With that in place, replaying the
same seed against the same ordered command list must reproduce an
identical event list — write that as a test the moment this shape lands
(see Testing Conventions). It is the cheapest, highest-signal test this
migration can produce: the first time it fails, something impure leaked
in — a wall-clock read, an unseeded random call, or non-deterministic
iteration over a `Dictionary`/`HashSet`.

An automated decision-maker (game AI, a scripted opponent) is not a special
case under this shape — it is a function that reads the simulation's query
members and returns a `TCommand`, submitted through the exact same
`Execute` entry point player input uses. AI logic in the bridge that
mutates simulation state directly instead of producing a command is the
same boundary violation as a Node deciding an outcome — see the ownership
rule above.

### One Simulation Root, or One Collaborator Per System?

The migration procedure below extracts one simulation class per system —
`HealthPool`, `SpawnTable`, whatever the system actually is — each owned
and constructed by the bridge Node that already used to hold its logic.
That's the right default: it matches an incremental, system-by-system
migration, and it needs no new coordination structure to work.

It stops being enough once systems need to react to *each other's*
changes — a shield system that must know when a hull system takes a hit, a
heat system that must know a weapon just fired — or once something needs
one atomic, whole-game snapshot: a single save file, a single state
broadcast to a networked peer, a single deterministic tick order across
every system. At that point, converge the separate collaborators under one
simulation root (commonly one `GameSimulation`-style class owning every
system, ticked by exactly one bridge Node) instead of leaving them as
siblings each bridge script reaches into independently. Don't build the
root pre-emptively — a project with two or three unrelated collaborators
and no cross-system reactions or save/network requirement gets nothing
from it but an empty coordination layer.

### Cross-System Communication Inside the Simulation

Once two simulation classes need to react to each other, three options
exist, and only one keeps the boundary intact:

- **Direct method calls between systems** (`Reactor` calling
  `_powerGrid.SetZonePower(...)` straight from its own `Tick()`) work, but
  tightly couple the two classes to each other's APIs, make mutation order
  implicit ("whoever calls last wins"), and make testing either system in
  isolation require constructing or mocking the other.
- **Godot signals** (`[Signal]`/`EmitSignal`) are worse here than direct
  calls, not better — they require a `GodotObject` base (an engine
  dependency inside the simulation, the exact thing this migration
  removes), dispatch on the main thread only (unsafe if the simulation
  ever moves to a background thread), box value-type payloads into
  `Variant`, can't be emitted in a unit test without a running
  `SceneTree`, and fire synchronously with no controlled drain point.
- **A plain C# event queue, internal to the simulation**, is the shape
  that holds: each system pushes small, immutable payloads (a `record` or
  `readonly record struct`) onto a shared queue; other systems drain the
  events that matter to them at a fixed point in the tick, never
  mid-mutation.

Start with the smallest version that fits the actual event count: a plain
C# `event Action<T>` per event type (the same shape as `Changed` in the
worked example above) or a single `List<TEvent>` cleared at the end of
each tick. Reach for a canonical typed-queue bus — one `Queue<T>` per
event type behind `Raise<T>(T evt)` / `TryPop<T>(out T evt)` — only once
the event *type* count or per-tick *volume* is large enough that a mixed
list becomes hard to read or measurably allocates. Whichever shape is
chosen, keep it inside the simulation assembly (still zero Godot
reference) and confine its use to the simulation's own tick — if the
simulation ever runs on a background thread, only that thread touches the
bus.

## Migration Procedure

Run this per gameplay system (one entity type, one mechanic) — not as a
single big-bang rewrite. Each system gets its own inventory, extraction,
tests, and verification pass before the next one starts.

1. **Inventory the target system.** Read every script involved. Classify
   each field and method as simulation state, simulation rule, or
   view/bridge. Any method that touches an `[Export]` field or anything in
   the `Godot` namespace is a bridge method by definition — even when it
   also contains a gameplay decision buried inside it. That buried decision
   is exactly what needs extracting; a bridge method calling into the
   engine is not itself evidence the whole method belongs in the bridge.

2. **Stand up the three-assembly skeleton**, if it doesn't already exist:
   a Sim class library with no Godot reference, and an xUnit (or matching)
   test project referencing only the Sim assembly. If a Sim-shaped project
   already exists as empty scaffolding, populate it — don't create a
   second one alongside it.

3. **Extract the simulation class first, behavior frozen.** Move the
   classified state and rules into a plain C# class in the Sim assembly,
   keeping every number and every branch exactly as it was. This move is
   *unprotected* — nothing pins its behavior yet. Do the mechanical move
   only; don't restructure it in the same step as moving it.

4. **Define the tick/command surface**, picking the shape from *Choosing
   the Command Surface Shape* below — continuous `Tick`/mutation methods,
   or a commands-in/events-out `Execute` entry point — whichever matches
   how the system is actually driven in play. Add read-only query members
   for the bridge to render from either way. This surface is the entire
   contract between the two layers. Keep it named for what each member
   does, not for how Godot happens to call it.

5. **Write the tests immediately, before any further restructuring.**
   Cover boundary conditions (zero, max, negative/invalid input), the
   actual gameplay rule being enforced — not just "does it compile" — and
   any quirk discovered mid-extraction that gets deliberately preserved
   rather than silently corrected. A behavior change found during
   extraction is a separate, deliberate decision for whoever owns game
   balance, not something a refactor gets to fix as a side effect; pin the
   current behavior with a comment saying so if it's staying as-is.

6. **Rewrite the Node script as a bridge.** It should now construct or
   receive the simulation object and call into it from
   `_PhysicsProcess`/`_Process`/input handlers. What "call into it" means
   depends on the surface chosen in step 4: for the continuous shape, poll
   the query members each frame and update sprites/UI directly; for the
   commands-in/events-out shape, enqueue the returned event list and
   animate one event at a time (a `Tween` per event is the common case),
   locking input until the queue drains — the simulation already knows the
   outcome the instant `Execute` returns, so anything the animation does
   afterward is playback, never a decision. Delete the logic that moved
   out either way. A bridge method that still contains the old gameplay
   decision *alongside* a call into the simulation means the extraction
   isn't finished, not that it's been made extra safe.

7. **Verify in this order:** `dotnet build` on the full solution (must
   stay clean — no new warnings, no new errors), `dotnet test` on the Sim
   test project (green, and actually exercising the new class — a
   build-only pass proves nothing), then open the editor and play the
   actual system to confirm behavior parity by feel. A split that compiles
   and unit-tests green but plays differently is not done.

8. **Commit the system as its own unit** before starting the next one, so
   a regression found in play-testing is one revert away, not a bisect
   through a project-wide rewrite.

9. **Repeat per system** until every gameplay-rule-bearing script is
   split. A script with no gameplay state — a camera-shake helper, a
   scrolling-background driver, a menu button handler — has nothing to
   extract. Leave those as plain Node scripts. Forcing a simulation class
   out of a script that has no gameplay state manufactures the split
   instead of completing it, and pads the Sim project with classes that
   exist to be counted rather than to be tested.

## A Worked Example

**Before** — gameplay rule and engine concerns mixed in one Node script:

```csharp
using Godot;

namespace Game;

public partial class Player : CharacterBody2D
{
    [Export] private ProgressBar _healthBar;
    [Export] private AudioStreamPlayer2D _hurtSound;

    private int _health = 6;
    private const int MaxHealth = 6;

    public void TakeDamage(int amount)
    {
        _health = Mathf.Clamp(_health - amount, 0, MaxHealth);
        _healthBar.Value = _health;
        _hurtSound.Play();

        if (_health == 0)
        {
            GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://GameOver.tscn");
        }
    }
}
```

**After** — the rule moves to the Sim assembly:

```csharp
// Game.Sim/HealthPool.cs — no Godot reference in this project
using System;

namespace Game.Sim;

public sealed class HealthPool
{
    public int Current { get; private set; }
    public int Max { get; }
    public bool IsEmpty => Current == 0;

    public event Action<int> Changed;

    public HealthPool(int max)
    {
        Max = max;
        Current = max;
    }

    public void Drain(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        int previous = Current;
        Current = Math.Clamp(Current - amount, 0, Max);
        if (Current != previous) Changed?.Invoke(Current);
    }
}
```

The bridge keeps only what needs the engine:

```csharp
// Game/Player.cs
using Godot;
using Game.Sim;

namespace Game;

public sealed partial class Player : CharacterBody2D
{
    [Export] private ProgressBar _healthBar;
    [Export] private AudioStreamPlayer2D _hurtSound;

    private HealthPool _health;

    public override void _Ready()
    {
        _health = new HealthPool(6);
        _health.Changed += OnHealthChanged;
    }

    public void TakeDamage(int amount)
    {
        _health.Drain(amount);
        _hurtSound.Play();
        if (_health.IsEmpty)
        {
            GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://GameOver.tscn");
        }
    }

    private void OnHealthChanged(int current)
    {
        _healthBar.Value = current;
    }
}
```

And the test, which never touches Godot:

```csharp
// Game.Tests/HealthPoolTests.cs
using Game.Sim;
using Xunit;

namespace Game.Tests;

public class HealthPoolTests
{
    [Fact]
    public void Drain_ClampsAtZero_DoesNotGoNegative()
    {
        var pool = new HealthPool(max: 6);
        pool.Drain(10);
        Assert.Equal(0, pool.Current);
        Assert.True(pool.IsEmpty);
    }

    [Fact]
    public void Drain_RaisesChanged_OnlyWhenValueActuallyChanges()
    {
        var pool = new HealthPool(max: 6);
        int callCount = 0;
        pool.Changed += _ => callCount++;

        pool.Drain(0);
        Assert.Equal(0, callCount);

        pool.Drain(2);
        Assert.Equal(1, callCount);
    }
}
```

Note what the bridge no longer does: no clamping math, no "what happens at
zero" decision. It calls `Drain`, plays a sound, and asks `IsEmpty`. Every
gameplay rule is in `HealthPool`, and every line of `HealthPool` is
provable by a test that runs in milliseconds with no editor open.

## Testing Conventions

- Default to xUnit for the Sim test project; match an already-established
  different framework instead of introducing a second one.
- Test the simulation class directly. The bridge should have no gameplay
  logic left to test once extraction is finished — a test that exercises
  the bridge is a sign the extraction didn't actually move what it claims
  to have moved.
- Name tests for the behavior, not the mechanism:
  `Drain_ClampsAtZero_DoesNotGoNegative`, not `TestDrain1`.
- Cover normal-range input, both boundaries, at least one invalid/edge
  input, and every quirk deliberately pinned rather than corrected during
  extraction.
- A green test suite with 0 assertions on the actual rule (only "does it
  construct") is not coverage — assert on the behavior the rule exists to
  guarantee.
- For a commands-in/events-out simulation, add a determinism canary: run
  the same seed against the same ordered command list twice and assert the
  two returned event histories are equal (`record` value-equality makes
  this a one-line `Assert.Equal`). This is the single highest-signal test
  in the whole migration — the first failure means something impure (a
  wall-clock read, an unseeded random call, non-deterministic iteration
  order) leaked into the simulation.

## Common Mistakes That Undo the Split

- **A fixed-tick clock bolted on "for correctness."** Determinism, replay,
  and rollback are a separate, larger decision (fixed-point math, an
  authoritative tick loop independent of `_PhysicsProcess`) that only pays
  for itself against an actual networking/replay requirement. Sim-view
  separation on its own runs `Tick()` from whatever cadence the bridge
  already uses — don't bundle the two decisions into one migration.
- **A second copy of simulation state cached in the Node** "for
  convenience." This reintroduces the exact two-owners bug the split
  exists to prevent — see the ownership rule above.
- **An interface wrapped around the simulation class** with exactly one
  implementation, added "for testability." The simulation class is already
  testable by construction — it has no Godot dependency to fake. An
  interface here adds indirection with nothing to justify it.
- **An `IRandomSource` abstraction around `System.Random`** when nothing
  needs a second source. `System.Random` is already injectable and
  seedable (`new Random(12345)` for a deterministic test); the interface
  only earns its cost once a real second implementation exists.
- **Testing through the bridge** instead of the simulation class directly.
- **Migrating a script with no gameplay state** just to make the Sim
  project's file count look complete — see step 9.
- **A global autoload/singleton that both layers write into.** A
  project-wide `GameState` (or similar) that the simulation *and* the
  bridge both mutate re-couples the two layers exactly like a direct
  reference would, one hop removed. The bridge holds exactly one reference
  to the simulation object it owns; the simulation never reaches back out
  through a project-wide singleton.
- **`async void` playback code.** A coroutine that drains an event queue
  and animates each entry should be `async Task` — called with a discard
  (`_ = PlayNext();`) if fire-and-forget is genuinely needed — wrapped in a
  try/catch that at least logs. An unhandled exception inside `async void`
  fails silently and can leave input locked for the rest of the session.

## Save and Replay, If the Project Wants Them

Not required by this migration, but worth naming because both fall out
nearly free once state changes only happen through the simulation's own
commands/mutation methods and randomness is seeded once at construction: a
save file can be **seed + ordered command/action log**, and "load" becomes
replaying that log into a fresh simulation instance instead of
deserializing a live object graph. Two landmines if this gets adopted:

- **`System.Random` cannot be read or restored.** Its internal state isn't
  exposed, so a naive save can't resume the same random sequence. Either
  save seed + log and replay on load (the option above), or swap in a
  small serializable PRNG (a xorshift/PCG implementation is a few lines)
  whose entire state is a couple of integers saved alongside everything
  else.
- **Polymorphic event/command hierarchies don't serialize with
  `System.Text.Json` by default** — it throws, or silently flattens to the
  base type, unless the hierarchy carries `[JsonPolymorphic]` plus one
  `[JsonDerivedType]` per concrete type. Every new event or command type
  needs its own line added, or serialization throws
  `NotSupportedException` the first time that type appears.

## Output Expectations

Per system migrated, report: which script(s) were split; the new
simulation class and its public tick/command/query surface; the test file
and what it actually covers, including any pinned quirk; confirmation that
`dotnet build` and `dotnet test` are both green; and confirmation of an
editor playtest for behavior parity. Flag any script deliberately left
un-migrated because it had no gameplay state to extract, so it isn't
mistaken for missed work.
