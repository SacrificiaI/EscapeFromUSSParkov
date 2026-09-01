# ADR-004: Godot version pin at 4.7.1-stable

## Status

Accepted, revisit opportunistically.

## Context

`project.godot` declares `config/features=PackedStringArray("4.7", "C#",
"Mobile")`, the `.csproj` pins `Godot.NET.Sdk/4.7.1`, and `.vscode/settings.json`
points at `Godot_v4.7.1-stable_mono_win64.exe` for both contributors.

Godot's current stable release is 4.7.1 (4.7 shipped 2026-06-18, adding C#
hot-reload among other changes; 4.7.1 is a patch release). Godot 4.8 is in
early development (a dev1 snapshot as of August 2026) and is not yet stable.

## Decision

Stay on 4.7.1-stable for this project. Treat any future upgrade (to 4.8+
once it reaches stable) as its own task with its own verification pass — a
new editor binary, a re-import pass over every asset and scene, and a check
that the analyzer packages and the `Godot.NET.Sdk` version bump cleanly —
not something folded into an unrelated feature or bugfix task.

## Consequences

### Positive

- Current on the latest stable line, including C# hot-reload, without
  taking on 4.8's pre-release instability.
- Stable, known-working toolchain for the remainder of this jam project's
  development.

### Negative

- Documentation and community examples that predate 4.7 may describe
  older behavior; a small but real risk of copying an example that doesn't
  apply to 4.7.1.

### Mitigations

- This ADR exists so the version in use is a visible, named decision
  rather than something an agent silently assumes is current. When citing
  Godot docs or community examples, verify they apply to 4.7 specifically
  where recent behavior has changed (signals, exports, and the C# API have
  been stable across 4.4–4.7, so most guidance transfers; check release
  notes for anything touching hot-reload or the export pipeline).
