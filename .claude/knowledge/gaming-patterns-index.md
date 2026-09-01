# Gaming Patterns Index

A problem → pattern map based on Robert Nystrom's *Game Programming
Patterns* — free to read at
[gameprogrammingpatterns.com](https://gameprogrammingpatterns.com/). Each
row below is self-contained (a one-line definition, not just a name); the
book link is optional further reading, not a dependency for using this
file.

## Problem → pattern map

| Problem | Pattern | What it is |
| --- | --- | --- |
| Sender shouldn't know its receivers; infrequent event (death, pickup, level end) | [Observer](https://gameprogrammingpatterns.com/observer.html) | One object (the subject) broadcasts an event; any number of listeners subscribe and react, without the subject knowing who's listening. |
| A class has more than two mutually exclusive modes | [State](https://gameprogrammingpatterns.com/state.html) | Each mode's behavior lives in its own state, with the object delegating to whichever state is currently active instead of branching on a pile of booleans. |
| Global project-wide service (event bus, score tracker) | [Singleton](https://gameprogrammingpatterns.com/singleton.html) / Autoload | Exactly one instance of a class exists for the whole program's lifetime, reachable from anywhere without being passed around explicitly. In Godot, an Autoload singleton is the built-in mechanism for this. |
| A player node doing physics + rendering + audio + AI all in one script | [Component](https://gameprogrammingpatterns.com/component.html) | Split one monolithic class into several small, single-concern objects (a physics component, an audio component, ...) owned by a shared parent, instead of one class doing everything. |
| Player input, replayable/undoable actions | [Command](https://gameprogrammingpatterns.com/command.html) | Wrap a request or action as an object instead of a direct method call, so it can be queued, logged, replayed, or undone. |
| Sender and receiver run on different ticks, or the same event could fire and need dedup | [Event Queue](https://gameprogrammingpatterns.com/event-queue.html) | Decouple *when* an event is raised from *when* it's handled by buffering events and processing them later, instead of dispatching synchronously. |
| Frame's updates must all see a consistent snapshot | [Double Buffer](https://gameprogrammingpatterns.com/double-buffer.html) | Keep two copies of a piece of state — one being read, one being written — and swap them atomically, so readers never see a half-updated value mid-frame. |
| Derived state expensive to compute, source changes rarely | [Dirty Flag](https://gameprogrammingpatterns.com/dirty-flag.html) | Track whether cached derived data is stale with a single flag; only recompute when something reads it *and* the flag is set, instead of recomputing on every change. |
| High-frequency spawn/despawn (bullets, particles) | [Object Pool](https://gameprogrammingpatterns.com/object-pool.html) | Pre-allocate a fixed set of reusable objects up front and recycle them, instead of `new`-ing and freeing on every spawn/despawn. |
| O(n²) proximity checks becoming a frame-budget problem | [Spatial Partition](https://gameprogrammingpatterns.com/spatial-partition.html) | Store objects in a structure organized by position (a grid, a quadtree) so "what's near this point" is a lookup instead of a scan of every object. |
| Hot loop slow for no obvious algorithmic reason | [Data Locality](https://gameprogrammingpatterns.com/data-locality.html) | Lay data out contiguously in the order it's processed, so the CPU's cache prefetches useful data instead of chasing scattered pointers. |
| A base class's skeleton algorithm is fixed, but a few steps vary per subclass | [Template Method](https://gameprogrammingpatterns.com/) (a Nystrom "Behavioral Patterns" sibling not covered by its own chapter) | The base class defines the overall flow and calls `protected virtual` hook methods; each subclass overrides only the hooks it needs to differ on. |

## How to use this map

1. State the actual problem in one sentence, not "what pattern should I
   use" — the concrete thing that's hard ("two unrelated scripts need to
   react to one event," "this entity has four mutually exclusive behaviors
   and it's turning into a boolean swamp").
2. Match it against the table above. Before adding a new instance of a
   pattern, check whether the project already has one — a project's own
   `CLAUDE.md` (or its own knowledge base) is the place that records which
   patterns are already in use and how, so a new instance matches the
   existing shape instead of introducing a second style for the same
   problem.
3. Recommend the least sophisticated pattern that solves the stated
   problem. Object Pool, Spatial Partition, Double Buffer, and Event Queue
   are real, well-documented patterns that only pay off past a certain
   scale (spawn churn, proximity-check count, concurrent readers/writers,
   cross-tick event volume) — don't recommend them speculatively on a
   codebase that hasn't reached that scale.
4. State the tradeoff, not just the answer. Every pattern trades something
   for its benefit — an event bus adds a layer of indirection; a state
   machine adds files for two states that a plain `bool` would cover. Name
   the specific cost for the specific problem being solved, not a generic
   warning.
5. If the plain, pattern-free code is simpler and the problem doesn't
   justify the indirection, say that instead of forcing a match.
