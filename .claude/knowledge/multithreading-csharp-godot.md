# Multithreading — Sim-Side Architecture (Godot 4 C#)

Godot's own docs are explicit that most projects never need this: threading
buys throughput and responsiveness at the cost of real correctness risk, and
the first two answers to "this feels slow" are almost always "measure it"
and "do less work," not "add a thread." This file exists for the point past
that — a background AI think, a procedural-generation pass, a save/export
that would otherwise cost a dropped frame — and is scoped specifically to
the **sim side** of a sim-view split: the thread boundary and the sim-view
boundary are the same line, because Godot's rendering/scene-tree side can
never leave the main thread, while pure simulation logic that doesn't touch
a `Node` can run anywhere.

Every claim below is either a direct quote from Godot's or Microsoft's own
docs, or explicitly marked as inferred from the engine's public source when
no doc page states it outright.

## The one Godot law

Godot's own docs state it plainly: **"Interacting with the active scene
tree is not thread-safe."** ([Thread-safe
APIs](https://docs.godotengine.org/en/stable/tutorials/performance/thread_safe_apis.html))
No `GetNode`, no `QueueFree`, no property set, no signal emission into the
tree from a worker thread — the failure mode is not a clean exception but
intermittent corruption, which is worse, because it can look like it works
in the editor and fail only in a release build under load.

The sanctioned escape hatch is deferral: `CallDeferred` (or
`Callable.From(...).CallDeferred()`) queues a call to run on the main thread
at the next safe point.

```csharp
// Wrong — touches a Node from a worker thread
Task.Run(() => _label.Text = "Done");

// Right — the write happens on the main thread
Task.Run(() => Callable.From(() => _label.Text = "Done").CallDeferred());
```

What's already safe, per the same page:

- **Most Global Scope singletons and Servers** — thread-safe by default.
- **`RenderingServer`, `PhysicsServer2D`/`3D`** — thread-safe once their
  respective "thread-safe operation" project setting is enabled.
- **`NavigationServer2D`/`3D`** — explicitly documented as "thread-safe and
  thread-friendly," no opt-in required.
- **Threaded resource loading** — `ResourceLoader.LoadThreadedRequest` /
  `LoadThreadedGetStatus` / `LoadThreadedGet` load scenes and assets on a
  background thread with progress polling. ([Background
  loading](https://docs.godotengine.org/en/stable/tutorials/io/background_loading.html))
  One documented trap: calling `LoadThreadedGet` before the load actually
  finishes blocks exactly like a synchronous `Load()` would — check the
  status first if the point was to avoid blocking.

What's explicitly not safe: modifying a shared `Resource` from multiple
threads at once, and — the one that matters most for gameplay code —
anything that touches a `Node`.

**Which threading API to reach for, in C#:** Godot ships its own
`WorkerThreadPool` for engine-internal use (physics, importers), but its own
docs point elsewhere for other languages: **"If using other languages (C#,
C++), it may be easier to use the threading classes they support."**
([Using multiple
threads](https://docs.godotengine.org/en/stable/tutorials/performance/using_multiple_threads.html))
For C#, that means `Task`, `Task.Run`, `Parallel`, and
`System.Threading.Channels` — the .NET tools below — not `WorkerThreadPool`,
except when interoperating directly with an engine-side system that
specifically expects it.

## Where this fits: the sim-view boundary is also the thread boundary

If simulation logic (state that changes, rules that apply to it) is kept
separate from view logic (Nodes, animations, rendering) — a *sim-view
split* — that same boundary is naturally where threading enters. A worker
thread can safely run pure sim logic because pure sim logic never touches a
`Node`; a worker thread can never safely touch the view side, because the
view side is nothing but `Node`s. Practically, that means: give a worker a
**frozen, read-only snapshot** of the state it needs, let it compute
something, and hand back a **result value** — never a live reference into
mutable state, and never a reference to anything in the scene tree.

```csharp
// A frozen snapshot — a value, not a live view. Records and read-only
// collections; nothing in here can be mutated by whoever holds it, so any
// number of worker threads can read one snapshot at once with nothing to
// synchronize.
public sealed record WorldSnapshot(
    Vector2 PlayerPosition,
    IReadOnlyList<EnemyView> Enemies);

public sealed record EnemyView(string Id, Vector2 Position, int Health);
```

## A concurrency ladder — climb only when pushed

Each rung costs more complexity than the last. Don't skip ahead of what the
actual problem needs.

| Rung | Tool | Reach for it when |
|---|---|---|
| **0 · Make it cheaper** | Precompute, cache, move from per-frame to per-event | Almost always the real fix — most "this needs threading" is really "this runs more often than it needs to." |
| **1 · Spread it** | Time-slice the work across several frames on the main thread | The work is bursty and divisible, and a few extra milliseconds per frame absorbs it without a thread at all. |
| **2 · One worker** | `await Task.Run(...)` for a single background operation | Any one operation (an AI decision, a procgen pass, a save) takes long enough to be felt as a frame — roughly 5ms or more. This rung alone covers most games that ever need threading. |
| **3 · Data-parallel** | `Parallel.For` over independent, read-only work | One operation is itself slow enough to split — scoring many independent candidates, batch-processing many independent items. |
| **4 · Dedicated worker** | A background loop reading commands and writing events over `Channel<T>` | The simulation needs to keep running continuously in the background, not just respond to one request at a time. |

## The core pattern: snapshot in, mutate on the main thread

The pattern behind rung 2, which covers the large majority of real cases:

```csharp
public partial class EnemyAiController : Node
{
    private WorldSnapshot Snapshot() => new(
        PlayerPosition: _player.GlobalPosition,
        Enemies: _enemies.Select(e => new EnemyView(e.Id, e.GlobalPosition, e.Health)).ToList());

    private async Task RunEnemyTurnAsync(string enemyId, CancellationToken ct)
    {
        WorldSnapshot snapshot = Snapshot();                    // frozen, taken on the main thread

        DecisionResult decision = await Task.Run(
            () => EnemyBrain.Decide(snapshot, enemyId, ct), ct); // runs on a pool thread

        // Execution resumes here, back on the main thread — not luck,
        // see the callout below. Touching Nodes from this point on is legal.
        ApplyDecision(enemyId, decision);
    }
}
```

While `Decide` runs on a pool thread, the main thread keeps pumping frames —
nothing visibly stalls. When the work finishes, execution resumes after the
`await`, and in Godot that resumption lands back on the main thread.

> **Why the resumption lands on the main thread.** As of Godot 4.3, the
> engine's .NET integration installs a `SynchronizationContext` on the main
> thread (`GodotSynchronizationContext`, wired up by an internal
> `GodotTaskScheduler`). An `await` captured on the main thread posts its
> continuation back through that context, and the engine drains that queue
> once per frame — the same contract WPF and WinForms have offered for
> years. **This isn't currently documented on a docs.godotengine.org page**
> (checked [C# vs GDScript
> differences](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html),
> which covers `await` syntax but not this mechanism) — it's inferred from
> the engine's own public source
> ([`GodotSynchronizationContext.cs`](https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/GodotSynchronizationContext.cs),
> [`GodotTaskScheduler.cs`](https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/GodotTaskScheduler.cs))
> and corroborated by community reports — verify against the actual engine
> version in use if this matters for correctness, not just convenience.
>
> A related rough edge, worth knowing before reaching for it explicitly:
> [Godot issue
> #40514](https://github.com/godotengine/godot/issues/40514) reports that
> `TaskScheduler.Current` inside a script is **not** `GodotTaskScheduler` —
> so code that explicitly calls
> `TaskScheduler.FromCurrentSynchronizationContext()` or schedules against
> `TaskScheduler.Current` may not behave as expected, even though plain
> `await` does. Prefer plain `await` over manual `TaskScheduler` usage for
> this reason alone.

The corollary: `ConfigureAwait(false)` is actively harmful in Godot
gameplay code that touches a `Node` afterward, even though it's standard
advice in library code. `ConfigureAwait(false)` deliberately discards the
captured context, so the next line after that `await` runs on a pool thread
— one `Node` access away from a Law 1 violation.

## C# concurrency primitives — quick reference

- **`Task.Run(...)`** — runs a delegate on the thread pool, returns a
  `Task`/`Task<T>` to `await`. The default choice for rung 2.
- **`Parallel.For(0, count, i => ...)`** — data-parallel iteration over
  independent work; pair with a `CancellationToken` via `ParallelOptions`
  for rung 3.
- **`System.Threading.Channels`** — an async-friendly producer/consumer
  queue, the standard tool for rung 4. `Channel.CreateUnbounded<T>()` or
  `Channel.CreateBounded<T>(capacity)`; write with
  `writer.WriteAsync(item)`, read with `await foreach (var item in
  reader.ReadAllAsync())`. Bounded channels apply backpressure — a slow
  consumer makes a fast producer's `WriteAsync` wait rather than growing
  memory without limit. ([Channels -
  .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels))
- **`CancellationToken`** — thread the same token through
  `Task.Run`/`Parallel.For`/`Channel` reads so a quit-to-menu or scene
  change can actually stop in-flight background work instead of leaking
  it. A background operation that's purely computing a value (nothing
  partially mutated yet) can be cancelled cleanly with nothing to unwind.
- **`System.Random` is not thread-safe.** Microsoft's own docs state it
  directly: if a `Random` instance is used from multiple threads without
  synchronization, "calls to methods that return random numbers return
  0." ([`Random`
  class](https://learn.microsoft.com/en-us/dotnet/api/system.random))
  **`Random.Shared`** (.NET 6+) is a thread-safe shared instance, documented
  as safe "to be used concurrently from any thread"
  ([`Random.Shared`](https://learn.microsoft.com/en-us/dotnet/api/system.random.shared))
  — but it is not seedable, so it's the right tool for background work
  whose exact outcome doesn't need to be reproduced later, and the wrong
  one the moment a replay or a second machine needs to compute the
  identical result (see the conditional section below).

## Common pitfalls

- **Fire-and-forget tasks.** `_ = Task.Run(...)` with nothing awaiting it
  loses any exception the task throws — the work silently stops and
  nothing reports why. If something genuinely can't be awaited at the call
  site, at minimum attach a continuation that logs a fault.
- **Blocking the main thread on a task.** `.Result`, `.Wait()`, or
  `.GetAwaiter().GetResult()` called from the main thread on a task whose
  own continuation needs to run on that same main thread's queue is a
  deadlock: the main thread is blocked waiting for a queue it should be
  draining. No exception, no stack trace, just a frozen game. `await`,
  with no exceptions to the rule, is the only safe way to consume a task
  from the main thread.
- **`async void` beyond a signal handler's outer shell.** An exception
  thrown inside an `async void` method can't be caught by any caller — it
  can crash the process outright. The one legitimate use is a thin signal
  handler that immediately delegates into a real `async Task` method
  inside a `try`/`catch`:

  ```csharp
  private async void OnButtonPressed()   // signal entry point only
  {
      try { await RunSomethingAsync(); }
      catch (Exception e) { GD.PushError($"Failed: {e}"); }
  }
  ```

- **Awaiting inside `_Process`/`_PhysicsProcess`.** These are called once
  per frame regardless of whether the previous call finished; an `async`
  override that awaits doesn't pause the engine, so long work re-enters on
  the next frame and piles up. Kick off long-running work from a discrete
  event (an input, a timer, a turn ending) behind a re-entrancy guard, not
  from a per-frame callback.
- **`ConfigureAwait(false)` in gameplay code.** See the callout above —
  correct in library code, wrong here whenever a `Node` gets touched after
  the `await`.

## If the simulation needs to be deterministic or replayable

Everything above is for the ordinary case: a background operation whose
exact numeric outcome doesn't need to match anything else. If the project
needs deterministic simulation — a replay system, or a networking model
where every machine must compute the identical result — parallel work adds
three specific new failure modes that the ordinary case doesn't have to
care about:

- **Completion order leaking into the result.** Anything that consults
  which parallel unit of work finished first (`Task.WhenAny` for "best so
  far," a shared variable raced between threads) makes the outcome depend
  on the scheduler, which differs machine to machine. Fix: write each
  parallel result into its own pre-sized slot, then reduce sequentially in
  a fixed order afterward.
- **Shared or unseeded randomness.** `System.Random` corrupts under
  concurrent use (above); `Random.Shared` fixes the corruption but can't
  be seeded, so two machines running the "same" simulation get different
  results. Derive any randomness needed inside parallel work from a
  combination of the simulation's seed and a stable identity (a unit's id,
  a tile coordinate) instead of any `Random` instance.
- **Unordered enumeration.** `Dictionary`/`HashSet` enumeration order is
  not a contract, and `Parallel.For`/`Parallel.ForEach` partitioning
  varies with core count. Any list that a decision is based on needs an
  explicit, stable sort (by id, by coordinate) before parallel work reads
  it, or the tie-breaks it produces will differ between runs.

This is real, additional complexity — don't take it on unless the project
has actually committed to deterministic simulation or lockstep networking
elsewhere in its architecture. It solves a problem most projects don't
have.

## When to actually reach for this

Threading is not free. Every thread is a new way for state to become
corrupted, in ways that are intermittent and hard to reproduce; dispatching
work to the pool costs real microseconds that a genuinely small job doesn't
recoup. Before adding any rung above 0:

1. **Measure it.** Confirm a specific operation is actually costing a
   visible frame, with a profiler, not a guess.
2. **Ask whether it should be running less often**, not just faster — most
   "this feels slow" turns out to be per-frame work that should be
   per-event, or a formula that should be a lookup table computed once.
3. **Audit the standard "things games thread" list against the project's
   own actual scale.** A technique that's essential in a 100-entity
   real-time battle can be pure overhead in a project with a handful of
   active enemies — the fix there is almost always rung 0 or 1, not rung 2
   or above.