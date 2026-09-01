---
paths:
  - "**/*.cs"
description: >
  Decision points where idiomatic Godot 4.x C# offers more than one valid
  approach, with the tradeoffs for each. This file states the options and
  when to prefer each — it does not assert which one any specific project
  has chosen. A project's actual choice belongs in that project's own
  CLAUDE.md (see the godot-init skill), stated as a decision, not
  re-derived here on every task.
---

# Godot C# Conventions

Godot's C# integration has several places where two engine-supported idioms
solve the same problem. Neither is wrong on its own, but mixing both within
one codebase for the same problem is a maintenance cost with no benefit.
Pick one per axis, record the choice in the project's `CLAUDE.md`, and match
it in new code instead of introducing a second valid-but-different idiom.

## Node references: `[Export]` fields vs. `GetNode<T>()`

- **`[Export]` fields**, wired in the editor and serialized as
  `node_paths=PackedStringArray(...)` in the owning `.tscn`, catch a broken
  reference at scene-load time (Godot reports a missing exported node) and
  show the wiring in the Inspector for anyone editing the scene.
- **`GetNode<T>("Path/To/Node")`** (or `%UniqueName` for a scene-unique
  node) needs no editor wiring and works for nodes that don't exist until
  runtime, but a typo'd path fails silently or throws only when that line
  actually runs, not at scene load.

```csharp
// [Export] — reference resolved and validated when the scene loads
[Export] private Sprite2D _sprite;

// GetNode — reference resolved (and validated) only when this line runs
private Sprite2D _sprite;
public override void _Ready() => _sprite = GetNode<Sprite2D>("Sprite2D");
```

Default to `[Export]` for anything present in the scene at edit time; reach
for `GetNode`/`%UniqueName` for a node that's genuinely dynamic (spawned at
runtime, resolved conditionally). For a reference to a node you don't own —
some other branch of the tree — prefer Godot groups
(`GetTree().GetFirstNodeInGroup("group_name")`) over a hardcoded tree path;
a tree path breaks the moment the scene gets restructured, a group name
doesn't.

## Signals: `[Signal]` attribute vs. C# `event Action<T>`

- **`[Signal]`** is required if GDScript or the editor (an `AnimationPlayer`
  call track, the Signals dock) needs to see or connect to the event.
  Godot's own source generator produces the emit method and the
  editor-visible signal.
- **C# `event Action<T>`** is a plain language-level event: faster (no
  `Variant` marshalling), compile-time typed, but invisible to GDScript and
  the editor's Signals dock.

If nothing outside C# — no GDScript, no editor-side connection — ever needs
to observe the event, a plain `event Action<T>` is the leaner choice. A
common shape for a pure-C# project is a single autoload exposing every
project-wide event as a C# `event`, with static helper methods that
null-conditionally invoke them:

```csharp
public partial class EventBus : Node
{
    public static event Action<int>? OnScoreChanged;
    public static void EmitOnScoreChanged(int score) => OnScoreChanged?.Invoke(score);
}
```

Whichever is chosen, apply it project-wide — don't reach for `[Signal]` for
one event and a C# `event` for the next without a reason tied to that
specific event needing GDScript/editor visibility.

## Subscribing and unsubscribing

Pair every `+=` subscription (typically in `_Ready`, or `_EnterTree` for
group registration) with a matching `-=` in `_ExitTree`. A plain C# `event`
has **no** Godot-side lifecycle awareness — subscribing and letting the
subscriber get `QueueFree()`d without unsubscribing leaves a dangling
delegate that fires into a freed node on the next invocation. `[Signal]`
connections behave differently (see
[godot-csharp-gotchas.md](../knowledge/godot-csharp-gotchas.md) for the
auto-disconnect rules and their lambda-capture exception), but the safe
default is the same: unsubscribe explicitly rather than relying on
lifecycle magic.

The one case that doesn't need a matching unsubscribe is a handler that
unsubscribes **itself** on first fire — a self-cleaning fire-once listener.
If a handler does that, comment why `_ExitTree` doesn't also remove it, so
a reader doesn't mistake it for an oversight.

## Entity variants: inheritance vs. scene composition

Two valid ways to express "several similar but distinct entities":

- **C# inheritance** — an abstract or `partial` base class with `protected
  virtual` hook methods, overridden per subclass. Works well when the
  variants share most of their behavior and differ only in a few
  well-defined hooks.
- **Scene composition** — each variant is its own `.tscn` instancing shared
  child scenes (`instance=ExtResource(...)`), with behavior assembled from
  reusable components rather than inherited. Works well when variants share
  structure more than behavior, or when a variant needs to skip a hook
  entirely rather than override it.

Don't force a variant that doesn't fit the shared base into inheriting it
anyway — a variant with a fundamentally different update loop or node
composition is a signal to leave it as its own class/scene rather than
contorting the base class to accommodate it.

## Namespaces

Godot's C# source generator doesn't require any particular namespace shape.
A single flat namespace for the whole project is simplest for a small
codebase; per-folder namespaces track a growing codebase's directory
structure at the cost of an `IDE0130` suppression (or per-folder
compliance) in `.editorconfig`. Either is fine — state which one the
project uses in `CLAUDE.md` so new files don't introduce a third shape.

## `sealed` by default

Mark a class `sealed` unless it's deliberately designed as a base for
inheritance — an unsealed class with no actual subclass is either dead
design intent or an invitation for someone to bolt on an override that a
composition-based extension would have served better. When a class *is*
meant as a base, its `protected virtual` members are its subclass-facing
API surface — document what each hook is for the same way a `public`
member would be documented.

## Naming conventions worth deciding explicitly

Godot's own APIs are PascalCase in C# (`Name`, `Velocity`, `QueueFree()`) —
match that for anything analogous. A few choices below aren't dictated by
the engine; whichever a project picks, apply it consistently and record it
in `CLAUDE.md`:

- Private field prefix: `_camelCase` is the .NET convention, and pairs
  naturally with an `[Export]` property that exposes the same value.
- `protected` members on a class meant for subclassing: some codebases keep
  `_camelCase` project-wide regardless of access level; others promote
  `protected` members to PascalCase to mark them as the subclass-facing API
  surface, distinct from truly private state. Either is defensible — pick
  one and don't mix them within the same base class.
- Constants: PascalCase matches .NET's own convention
  (`Environment.NewLine`, not `ENVIRONMENT_NEW_LINE`); `SCREAMING_CASE` is
  common in C-family languages generally but isn't the .NET default.
- `var`: enabling or disabling `csharp_style_var_for_built_in_types` in
  `.editorconfig` is a project-wide choice — don't let it drift file by
  file.
- Named arguments for a bare `bool` at a call site
  (`SetDeferred(Area2D.PropertyName.Monitoring, value: true)`) remove the
  single largest source of "what does `true` mean here" ambiguity in a
  method call, and cost nothing to add.

## Deferred calls: prefer the generated `MethodName`/`PropertyName` caches

`CallDeferred(Node.MethodName.QueueFree)` and
`SetDeferred(Area2D.PropertyName.Monitoring, value: true)` resolve to the
same `StringName` Godot's engine API needs, but go through the
source-generated `MethodName`/`PropertyName` nested class instead of a raw
string literal — a typo in the literal form (`CallDeferred("QueuFree")`)
fails silently at runtime; the same typo in the generated form fails to
compile. Prefer the generated form whenever the call target is a Godot
engine member. `nameof(...)` is the right choice only for a same-class C#
method, which `MethodName` doesn't cover.

## State machines: `AnimationTree` vs. a hand-written `State` pattern

Godot's `AnimationTree` state machine (driven from code via
`_animationTree.Set("parameters/conditions/X", value: true)` and read via
`_animationTree.Get("parameters/playback")`) already implements Nystrom's
[State](https://gameprogrammingpatterns.com/state.html) pattern for
anything whose states are also visual/animation states. Reach for it before
writing a hand-rolled `enum`-and-`switch` or class-per-state FSM when the
states in question already correspond to distinct animations or visual
transitions. A hand-written State pattern earns its keep when the states
are **not** primarily visual — a network connection lifecycle, a save/load
flow, a turn-based game's phase sequence.

## Moving platforms: pick the recipe by how the platform moves

Two working recipes, chosen by `sync_to_physics` — don't mix them on the
same body, they fight and stutter each other:

- **Path-driven** — a `PathFollow2D` riding an editor-authored `Path2D`,
  advanced via `Progress += speed * (float)delta` in `_PhysicsProcess`,
  with `sync_to_physics` **off** (the script moves the body itself every
  physics tick). Best for a route that's easiest to author visually as a
  curve.
- **Tween-driven** — an `AnimatableBody2D` eased between two `Marker2D`
  points with a `Tween`, `sync_to_physics` **on**. Best for a simple
  point-to-point ping-pong where a full `Path2D` would be overkill. If
  `_Ready` starts the tween immediately, awaiting one `ProcessFrame` and
  one `PhysicsFrame` first avoids first-frame jitter — the physics server
  can otherwise start reporting the body's motion before it's finished
  registering the body.

## Save data: `Resource` subclasses with `[Export]` fields

A `Resource` subclass is Godot's native save format —
`ResourceSaver.Save`/`ResourceLoader.Load` serialize it directly. `[Export]`
on a `Resource` field is not optional decoration: it's what registers the
field as a Godot property via the C# source generator in the first place. A
plain field with no `[Export]` is invisible to Godot's property system —
`ResourceSaver`/`ResourceLoader` never see it, so it silently reverts to its
C# default on load with no error.

```csharp
public partial class PlayerSaveData : Resource
{
    [Export] public int Lives { get; set; }
    [Export] public Godot.Collections.Array<int> Scores { get; set; } = new();
}
```

## Pausing: `GetTree().Paused` plus opt-out via `ProcessMode`

`GetTree().Paused = true` freezes every node whose `ProcessMode` is the
default (`Inherit`, which resolves to `Pausable`). Anything that must keep
running through that pause — a HUD reading a "continue" input, a UI
transition — sets `ProcessMode = ProcessModeEnum.Always` on itself in
`_Ready`. Opt individual nodes out explicitly rather than routing pause
state through a manual `if (paused) return` check at the top of every
method that shouldn't run during pause.