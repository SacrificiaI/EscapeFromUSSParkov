# Godot 4.x C# Gotchas

Verified against the official docs for the current stable line
([C# signals](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html),
[C# vs GDScript differences](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html)).
These apply to any Godot 4.x C# project, regardless of whether it uses
`[Signal]` or a plain C# `event` (see
[godot-csharp-conventions.md](../rules/godot-csharp-conventions.md) for
that choice) — worth knowing before reaching for `[Signal]`/`Connect` in new
code or a tutorial's example.

## Lambda captures defeat auto-disconnect

If you connect a Godot `[Signal]` to a lambda that captures variables,
`+=`/`Connect` can't tell the lambda is tied to the instance that created
it, so Godot won't auto-disconnect it when that instance is freed — the next
invocation can throw `ObjectDisposedException`. `Callable.From` doesn't fix
this either; it doesn't affect `Delegate.Target`. If a lambda captures state
and needs to survive the emitter's lifetime, either use a named method (so
`-=` in `_ExitTree` has something concrete to remove) or track the
`Callable` and disconnect it explicitly.

The exception: an inherently one-shot emitter that self-cleans up needs no
`-=` at all. Subscribing a capturing lambda to a one-shot `Tween.Finished`
signal on every call, with no matching unsubscribe, is correct as long as
each `Tween` came from a fresh `CreateTween()` call — per Godot's own
`Tween.IsValid()` docs, "a Tween might become invalid when it has finished
tweening, is killed, or when created with `Tween.new()`." A finished,
non-looping Tween can't re-emit `Finished`, so the lambda fires exactly once
no matter how many times the call runs — there's no accumulating list of
dead subscriptions the way there would be on a long-lived node. The rule
above is about signals on long-lived nodes; it doesn't apply to a callback
on an object that's already scoped to die after one emission.

## Custom `event Action` signals never auto-disconnect

A plain C# `event` has **no** Godot-side lifecycle awareness at all —
subscribing with `+=` and letting the subscriber get `QueueFree()`d without
a matching `-=` leaves a dangling delegate reference that fires into a
freed node next time. Any project using C# `event` for cross-node
communication needs an explicit `_ExitTree` unsubscribe for every `_Ready`
subscription (see
[godot-csharp-conventions.md](../rules/godot-csharp-conventions.md)).

## `partial` is required by source generators, not stylistic

Every Godot-derived C# class must be `partial`. Godot's source generators
inject signal/binding/export code into a second half of the same class;
this isn't optional boilerplate. A single-part `partial` class — one with
no other file completing it — is normal in a Godot project and not a code
smell, since the generator provides the other half at build time.

## `QueueFree()` vs `Free()` vs the GC

Never rely on the C# garbage collector to clean up a `GodotObject`
(`Node`, `Resource`, etc.) — the GC doesn't know about Godot's native-side
memory and won't free it. `QueueFree()` already "queues a node for deletion
at the end of the current frame" (Godot's own description) — it does **not**
need an extra `CallDeferred(Node.MethodName.QueueFree)` wrapper; that
double-defers a call that was already safe, adding a frame of delay for
nothing. `Free()` (immediate, synchronous) is rarely appropriate and never
from a physics-driven callback — freeing a `CollisionObject2D`/`3D` while
the physics server is still mid-step is a documented Godot error ("Removing
a CollisionObject node during a physics callback is not allowed"). A plain
`QueueFree()` call from inside a signal handler (an `AreaEntered` callback,
for example) is already correctly deferred by the engine — wrapping it in
`CallDeferred` is harmless but unnecessary, not a pattern worth copying
forward.

## Physics/collision *state* (not the node itself) still needs deferring

`QueueFree()` is self-deferring, but changing a `CollisionObject2D`'s other
properties from inside its own physics callback is not automatically safe —
`Monitoring`, `Monitorable`, and a `CollisionShape2D.Disabled` toggle must go
through `SetDeferred`, or Godot rejects the change outright while it's still
flushing that physics step's queries:

```csharp
// Correct — deferred so it applies after the current physics step finishes
SetDeferred(Area2D.PropertyName.Monitoring, value: true);
```

Use this form for any `Disabled`/`Monitoring`/`Monitorable` write triggered
from a signal callback, even though a plain field write would compile fine.

## `_Process`/`_PhysicsProcess` take `double delta`, not `float`

Godot 4's C# API signature is `_Process(double delta)` /
`_PhysicsProcess(double delta)`. Cast to `float` at the point of use if
downstream math is `float`-based (`float dt = (float)delta;`), not in the
method signature.

## `Godot.Collections` vs `System.Collections` marshalling

`Godot.Collections.Array`/`Dictionary` (and their generic `<T>` forms) exist
for interop with the engine and GDScript — they carry marshalling overhead.
For pure C#-to-C# data that never crosses into a `Variant`-typed API,
`System.Collections.Generic` types are lighter. Reach for `Godot.Collections`
only when an engine API specifically requires it (an `[Export]`ed
`Resource` field being one common case where it does).

## String encoding mismatch

C# `string` is UTF-16; Godot's internal `String` is UTF-32. This mostly
doesn't matter until doing byte-level or encoding-sensitive text work, but
it's worth knowing before a save-file or text-processing system touches raw
file encoding.

## Collision needs a matching layer *and* mask, or it fails silently

An `Area2D`/`PhysicsBody2D` signal (`AreaEntered`, `BodyEntered`) not firing
is very often not a code bug. **Layer** answers "what am I?" (the physics
layers this body occupies); **mask** answers "what do I scan for?" (the
layers this body reports overlaps with). For A to detect B, B's layer must
appear in A's mask — get this wrong and there's no error, no warning, just
silence. Naming physics layers in `project.godot`
(`2d_physics/layer_1="platforms"`, for example) so the inspector checkboxes
read as words instead of numbers makes this class of bug far faster to
spot. When a collision/area signal looks correctly wired in code but never
fires, check layer/mask configuration in the editor **before** re-reading
the C#.

## An event bus only reaches subscribers already in the tree

A C# `event`-based event bus (or any Godot signal) delivers to whatever is
currently subscribed — nothing more. A listener that hasn't been
instantiated yet, or hasn't reached `_Ready` yet, simply never subscribed,
so the emit is a silent no-op for it: no error, because nothing was
disconnected — nothing was ever connected. If an event "does nothing,"
confirm the listener actually exists in the running scene and got past
`_Ready` before the emit, before assuming the wiring is wrong. Decoupling
(the entire point of an event bus) hides this failure mode along with the
coupling it removes.

## Guard a reference that can outlive its holder with `IsInstanceValid`

`QueueFree()` disposes the underlying native Godot object; a cached C#
reference to it becomes a tombstone that throws `ObjectDisposedException` on
next access, not a null. Any reference that can be freed independently of
whatever's holding it — a cached target, a cached player, a cached parent —
needs an `IsInstanceValid()` guard before use:

```csharp
if (!IsInstanceValid(_targetRef))
{
    QueueFree();
    return;
}
```

A common case: an enemy caches a reference to the player in `_Ready`, and
the player can die (and be freed) before the enemy does.

A related but distinct guard is `IsInsideTree()`, for a *deferred or queued*
callback rather than a *cached reference*: Godot can still deliver a
callback that was already queued after the node that queued it has left the
tree. A callback that mutates `this` state should check `IsInsideTree()`
before acting, for the same reason a cached reference to something *else*
needs `IsInstanceValid()` — the object the callback assumes still exists
might not.

## Phantom errors after a rename: rebuild before debugging

Godot's C# integration leans on generated partials
(`*_ScriptMethods.generated.cs`) layered onto every Godot-derived class.
Rename a class or move a namespace and those generated files can briefly
desync from the hand-written half, producing a wall of errors that don't
correspond to anything actually wrong in the source — they can number in the
thousands and point at unrelated code (animation, unrelated types). If a
pile of new errors appears immediately after a rename or a namespace move
and the messages don't match the actual change, rebuild (or restart the
editor) once before spending time investigating. Only debug errors that
survive a clean rebuild.

## Input event propagation order

Godot dispatches an input event through a fixed sequence, and a handler that
marks the event handled (`Viewport.SetInputAsHandled()` /
`Control.AcceptEvent()`) stops it from reaching the rest of the chain:
`_Input` → `_GuiInput` (`Control` nodes only) → `_ShortcutInput` (key/
shortcut/joypad-button events only) → `_UnhandledKeyInput` (keyboard only) →
`_UnhandledInput` → physics object picking's `_InputEvent` (collision
shapes only, via `Area2D`/`PhysicsBody2D`, and only if picking is enabled).
Global shortcut handling belongs in `_ShortcutInput` rather than folded into
`_UnhandledInput`. `_UnhandledInput`/`_UnhandledKeyInput` are both
downstream of `_GuiInput` — a `Control` node (a menu, a HUD prompt) gets
first refusal on an event, so a `Control` that consumes an event won't leak
it into gameplay input handled further down the chain.

## Naming: C# is PascalCase where GDScript is snake_case

`set_name("x")` in GDScript is `Name = "x"` in C#. Global helpers move onto
static classes: `Mathf` for math (`Mathf.Abs`, `Mathf.Clamp`), `GD` for
engine utilities (`GD.Print`, `GD.Randi`, `GD.RandRange`). Don't introduce
snake_case identifiers when translating an example from a GDScript-first
tutorial or reference.
