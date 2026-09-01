---
paths:
  - "**/*.cs"
description: >
  Hot-path allocation and Godot-specific performance rules for C# gameplay
  code. Distinguishes free habits (apply everywhere, no cost) from
  complexity-adding tools (hot-path or measured only).
---

# Performance

Two different kinds of "performance rule" live in this file, and treating
them the same either overcomplicates trivial code or lets easy wins slide:

- **Free habits** — cost nothing extra to write correctly the first time and
  don't add complexity. Apply these by default, everywhere, not only on
  measured hot paths. Retrofitting them later is real, avoidable work — get
  them right the first time.
- **Complexity-adding tools** — `Span<T>`, `stackalloc`, `ref`/`in`,
  `readonly struct`, `ArrayPool<T>`. These trade simplicity for speed. Apply
  them only on a genuine hot path (`_Process`, `_PhysicsProcess`, and
  anything they call) or after profiling — see
  [priorities.md](priorities.md). Don't reach for one of these without a
  concrete allocation or measurement to justify it.

The free habits below matter even on modest hardware — integrated GPUs, 8 GB
RAM, no dedicated graphics memory to lean on — where the complexity-adding
tools stay gated on an actual profiling result regardless of target
hardware.

---

## Free habits — apply by default, not just on hot paths

### Exclusive data ownership

One piece of mutable state, one owner that writes it. Everything else only
reads. This is a correctness discipline as much as a performance one — it's
grouped here because it costs nothing to follow and prevents a class of bug
that's genuinely hard to track down later: a "lost update," where two
systems write the same field in the same frame and one write silently
clobbers the other.

A common shape for this: an autoload owns its own event state, and a node
exposes derived flags (`IsHurt`, `IsFalling`) as public read-only properties
backed by a private field only that node itself mutates. Keep it deliberate:
if a second script needs to change something another script owns, route it
through a method call or an event — never write directly to another type's
field or another autoload's public state from outside.

### Prefer a plain loop over LINQ for anything processed more than once per session

A raw `for`/`foreach` is not meaningfully harder to write than the LINQ
equivalent, and it allocates nothing — no enumerator, no closure, no
intermediate list. Default to the loop for anything that runs on every
frame, every physics tick, or every time a frequently-firing handler runs —
not only after profiling flags it as slow.

```csharp
// Prefer this by default — no allocation, equally readable
int aliveCount = 0;
for (int i = 0; i < enemies.Count; i++)
{
    if (enemies[i].IsActive)
        aliveCount++;
}

// Over this, for anything beyond a true one-shot
int aliveCount = enemies.Count(e => e.IsActive);
```

This is a default, not a ban. A one-shot `_Ready`-time call like
`GetChildren().OfType<TextureRect>().ToList()` runs exactly once per scene
load, and LINQ there is the right call: it's more readable and the cost is
paid once, ever. The line is *repetition*, not "is this technically inside
`_PhysicsProcess`": code invoked once at startup can use whatever reads
best; code invoked every frame, or every time a handler fires during normal
play, should default to a plain loop from the start rather than waiting to
be flagged.

### Keep hot-path collections concretely typed, not `IEnumerable<T>`

`List<T>.GetEnumerator()` returns a `struct` specifically so a `foreach` (or
indexed `for`) over a `List<T>`/array costs nothing — the enumerator lives on
the stack, never the heap. That guarantee only holds when the compiler knows
the concrete type at the call site. Access the same collection through an
`IEnumerable<T>`-typed field, parameter, or return value instead, and the
compiler can only call the interface's `GetEnumerator()`, which boxes that
struct onto the heap to satisfy `IEnumerator<T>`. This is exactly why the
loop in the previous section is genuinely zero-allocation while
`enemies.Count(e => e.IsActive)` isn't: `Count(Func<T, bool>)` isn't a
`List<T>` member, so it resolves to the LINQ extension method, which only
sees `enemies` as `IEnumerable<T>`.

```csharp
// Wrong — parameter type forces enumerator boxing on every call
private static int CountAlive(IEnumerable<Enemy> enemies) { /* ... */ }

// Right — concrete type, the enumerator stays a stack struct
private static int CountAlive(List<Enemy> enemies) { /* ... */ }
```

Keep hot-path fields, parameters, and return types declared as their
concrete collection type (`List<T>`, `T[]`), not `IEnumerable<T>`/
`ICollection<T>`. Reserve the interface types for one-shot setup code where
the allocation doesn't matter — same case as the `_Ready`-only
`OfType<TextureRect>().ToList()` call above.

### `StringBuilder` for strings built from more than one piece

A single interpolated string (`$"HP: {hp}"`) is one allocation and is fine
anywhere. But building a string across multiple appends or a loop — a debug
dump, a composed label, a save-file line — should use `StringBuilder` from
the start. Repeated `+=` or interpolation-in-a-loop allocates a new
intermediate string on every iteration:

```csharp
// Wrong — allocates a new string every loop iteration
string summary = "";
foreach (var zone in zones)
    summary += $"{zone.Name}: {zone.Power} | ";

// Right — one allocation, written this way from the start
var sb = new StringBuilder(64);
foreach (var zone in zones)
{
    sb.Append(zone.Name);
    sb.Append(": ");
    sb.Append(zone.Power);
    sb.Append(" | ");
}
string summary = sb.ToString();
```

### Change-detection before rewriting a `Label`/`Text` property every frame

If a value is only checked every frame but rarely changes, guard the write
so the string allocation happens only when the value actually changed —
free to add, and worth doing from the start for any debug or HUD label
reformatted on a timer or every tick:

```csharp
private float _lastDisplayedVelocityY = float.NaN;

private void UpdateDebugLabel(float velocityY)
{
    if (velocityY == _lastDisplayedVelocityY) return;
    _lastDisplayedVelocityY = velocityY;
    _debugLabel.Text = velocityY.ToString("F2");
}
```

This is the [Dirty Flag](https://gameprogrammingpatterns.com/dirty-flag.html)
pattern in miniature — `_lastDisplayedVelocityY` is the flag, the comparison
is the check, and the `Text` write is the deferred work. See
[gaming-patterns-index.md](../knowledge/gaming-patterns-index.md) for the
pattern's full entry.

### Bounded loops

Every `while` loop must be provably bounded — either by iterating a fixed
collection (`for (int i = 0; i < enemies.Count; i++)`) or, if a `while` is
genuinely needed, an explicit hard iteration cap. An unbounded loop that
never converges (`while (IsOverlapping()) Reposition();` with no escape
case) doesn't degrade gracefully the way a slow web request does — a single
client process has no load balancer to fail over to, so a hang there is the
entire, only, running instance of the game. This is free: a bounded loop is
not harder to write than an unbounded one, just written with the exit
condition considered up front. Adapted from Rule 2 of Gerard Holzmann's
(NASA/JPL) ["The Power of Ten — Rules for Developing Safety-Critical
Code"](https://spinroot.com/gerard/pdf/P10exp.pdf), IEEE Computer, June
2006 — a project that records its own architecture decisions may have a
fuller rule-by-rule evaluation of that paper in its own knowledge base.

### No unbounded recursion on the hot path

Recursion itself isn't banned — it's a fine tool for a one-shot tree walk or
a small decision routine. But recursion (or any loop) inside
`_Process`/`_PhysicsProcess` or anything they call needs a provable depth
bound, because a stack overflow or runaway call chain there happens during
play, at the worst possible moment, not during a load screen.

### `Debug.Assert` for invariants you already believe are true

`Debug.Assert` compiles out of Release builds entirely — zero production
cost — and catches a broken assumption during development instead of letting
it silently produce a wrong result. Use it for invariants a method already
relies on, not for input validation — reserve actual validation for real
trust boundaries (network input, a save file loaded from disk, modded
content); see [priorities.md](priorities.md) on not adding checks for
scenarios that can't happen in a project with no such boundary.

```csharp
private void ReduceLives(int livesReduction)
{
    Debug.Assert(livesReduction > 0, "ReduceLives should never be called with <= 0");
    _lives -= livesReduction;
    EventBus.EmitOnPlayerHit(_lives, isShaking: true);
}
```

Worth adding where a method already has an implicit assumption (a parameter
that should always be positive, a state that should never be reached) —
not something to retrofit everywhere at once.

---

## Complexity-adding tools — hot path or measured only

**Hot paths are:** `_PhysicsProcess`, `_Process`, and anything called
directly from them.

### No heap allocation on the hot path

Never `new` a class or allocate a collection inside `_Process`/
`_PhysicsProcess` or a method they call.

```csharp
// Wrong — allocates a new collection every physics tick
var nearby = new List<Enemy>();

// Right — reuse a pre-sized buffer of an unmanaged type where possible.
// Enemy is a Node (managed reference type), so it cannot live in a
// stackalloc'd Span — that only works for unmanaged types like int,
// float, or a plain struct with no reference fields.
Span<int> nearbyIndices = stackalloc int[MaxNearby];
```

### `readonly struct`, `ref`/`in`, `Span<T>` — when they're actually worth it

These are real tools, not defaults to reach for. Apply them when a `struct`
is large (3+ fields, or contains a `Vector2`/`Vector3`) **and** it's passed
around on a hot path:

```csharp
// Wrong — every method call on an `in` parameter triggers a hidden
// defensive copy, because the compiler can't prove the struct is immutable.
public struct HitData { public int Damage; public Vector2 Direction; }

// Right — `readonly struct` lets the compiler skip the defensive copy.
public readonly struct HitData
{
    public int Damage { get; init; }
    public Vector2 Direction { get; init; }
}

private void ApplyHit(in HitData hit) { /* ... */ }
```

### When NOT to apply these tools

Don't apply `ref`, `in`, `readonly struct`, or `Span<T>` to code that runs
once per event — input handling, UI callbacks, scene transitions,
`_Ready`/`_ExitTree` subscription wiring. The complexity isn't earned there.
The threshold is the hot path: if it isn't called every tick, write it
plainly. This restriction is specific to these tools — it does not apply to
the free habits above, which cost nothing extra anywhere.

---

## Godot-specific

- Cache node references as `[Export]` fields (see
  [godot-csharp-conventions.md](godot-csharp-conventions.md)); don't call
  `GetNode<T>()` with a string path inside a hot-path method.
- Signals/C# events are fine for infrequent events (death, pickup, level
  transition). Don't route per-frame state through them — use direct field
  reads/writes instead.
- `_PhysicsProcess` runs on Godot's physics tick (60 Hz by default). A
  project with no separately-clocked simulation tick has no
  fixed-vs-variable timestep hazard to guard against; one that does (a
  lockstep or replay-driven simulation) needs that split stated explicitly
  in its own doctrine, not assumed here.
- Avoid `Callable.From` with a capturing lambda inside a hot-path method —
  each call allocates a new delegate.

## C# code quality (still applies, still cheap)

- `readonly` on fields set once and never mutated.
- `sealed` on classes not designed for inheritance (see
  [godot-csharp-conventions.md](godot-csharp-conventions.md)).
- Explicit access modifiers always — never rely on default `private`.
- No `dynamic`, no reflection in gameplay code.
