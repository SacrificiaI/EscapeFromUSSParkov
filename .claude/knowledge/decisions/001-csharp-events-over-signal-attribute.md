# ADR-001: C# `event Action<T>` over Godot `[Signal]`

## Status

Accepted (already implemented throughout the codebase; documented here so
the reasoning survives outside the `SignalHub.cs` comment block).

## Context

Godot 4 C# supports two ways to declare a signal-like notification: the
native `[Signal] public delegate void XEventHandler(...)` attribute (visible
in the editor, callable from GDScript, backed by `EmitSignal`/`Connect`), or
a plain C# `event Action<...>` with ordinary `+=`/`-=` subscription and
`?.Invoke()` emission.

This project is 100% C#, with no GDScript scripts and no need for signals to
appear in the editor's Node signal panel. `SignalHub`
([SignalHub.cs](../../../globals/SignalHub.cs)) is the single global event
bus and every gameplay event goes through it or through local per-node
signals (`_hitBox.AreaEntered`, `_timer.Timeout` — these are Godot-native
signals on built-in node types, unaffected by this decision).

## Decision

Use plain C# `event Action<...>` for all project-defined notifications
(global bus events on `SignalHub`, and any future custom per-node
notification). Reserve `[Signal]` for the case that doesn't apply here: a
signal that must be wired from the editor's Node panel or received by
GDScript code.

```csharp
// SignalHub.cs — the established pattern
public event Action<int, bool> OnPlayerHit;

public static void EmitOnPlayerHit(int lives, bool isShaking)
{
    Instance.OnPlayerHit?.Invoke(lives, isShaking);
}
```

## Consequences

### Positive

- Compile-time type safety: a wrong argument type or count is a build error,
  not a runtime `Callable` mismatch.
- Faster invocation — no `Variant` marshalling, no string-keyed signal
  lookup.
- No editor-side wiring to keep in sync with code; the whole event contract
  lives in `SignalHub.cs`.

### Negative

- Invisible in the Godot editor's Node signal panel — a newcomer inspecting
  a scene in the editor won't see these connections; they're discoverable
  only by reading the C# subscriber code.
- If this project ever adds GDScript (e.g. a modding layer, or a
  designer-authored script), those events are unreachable from GDScript
  without an additional `[Signal]`-based bridge.

### Mitigations

- [godot-csharp-conventions.md](../../rules/godot-csharp-conventions.md)
  documents the subscribe-in-`_Ready`/unsubscribe-in-`_ExitTree` discipline
  that keeps these events debuggable despite the lack of editor visibility.
- If GDScript interop becomes a real need, that's a deliberate future
  decision (a `[GlobalClass]`-exposed bridge type), not a reason to switch
  the whole project to `[Signal]` now.
