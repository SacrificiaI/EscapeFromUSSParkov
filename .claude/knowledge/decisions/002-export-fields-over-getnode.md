# ADR-002: `[Export]` fields over `GetNode<T>()` / `%UniqueName`

## Status

Accepted (already implemented throughout the codebase; zero exceptions
across all 26 scripts).

## Context

Godot C# offers at least three ways for a script to get a reference to a
child node: `GetNode<T>("RelativePath")` (string path traversal, resolved at
runtime), `%UniqueName` / `GetNode<T>("%UniqueName")` (unique-name lookup,
still a runtime string resolution), or a typed `[Export]` field wired to a
specific node in the editor (resolved once, at scene instantiation, with no
string parsing).

This codebase uses the third option exclusively. `Player.tscn`,
`GameHud.tscn`, and every other scene serialize their `[Export]` wiring via
`node_paths=PackedStringArray(...)` on the root node.

## Decision

Declare node references as typed `[Export] private FieldType _fieldName;`
and wire them in the Godot editor inspector. Do not use `GetNode<T>()` with
a string path, and do not use `%UniqueName`, for references a scene already
owns structurally.

Cross-scene lookups (finding a node this script doesn't structurally own,
like an enemy finding the player) go through Godot groups instead:
`GetTree().GetFirstNodeInGroup(GameConstants.GroupPlayer)` — see
[EnemyBase.cs](../../../scenes/Enemies/EnemyBase/EnemyBase.cs). This is a
different problem (finding an unrelated node) from the one `[Export]`
solves (referencing a child this scene already contains).

## Consequences

### Positive

- No runtime string-path resolution or scene-tree traversal cost, however
  small, on the reference itself.
- A broken reference (renamed/moved node) surfaces immediately in the editor
  inspector as a dangling export, not as a silent `null` from `GetNode` at
  runtime.
- The dependency between a script and its expected child nodes is visible
  and editable in one place (the Inspector), not scattered as string
  literals through the script.

### Negative

- Rewiring a scene's internal structure (renaming a child node) requires
  re-dragging the export in the editor, not just a text edit — a small
  friction cost compared to updating a `GetNode` string path.
- A new contributor unfamiliar with the convention might reach for
  `GetNode<T>()` out of habit (it's the more commonly tutorialized approach).

### Mitigations

- [godot-csharp-conventions.md](../../rules/godot-csharp-conventions.md)
  states the rule explicitly with a citation to a real scene, so it's easy
  to point at when reviewing new code.
