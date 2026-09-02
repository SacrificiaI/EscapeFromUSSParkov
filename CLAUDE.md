# Escape from USS Parkov

A 2D top-down action prototype built by a two-developer team in Godot 4.7
+ C#. You are a colonial marine escaping the alien-infested USS Parkov:
reach the escape pods before the ship's creatures kill you. Title from
`config/name` in
[`EscapefromUSSParkov.View/project.godot`](EscapefromUSSParkov.View/project.godot).
Single-player, no networking. It is Phase 1 of an independent practice
series — the first test of transferring course knowledge into the team's
own code without tutorial rails — so it is deliberately kept as a learning
artifact: optimize for a clear teaching diff, not for scaling.

**The design authority is the Obsidian vault, not this repo:**
`G:/GameDev stuff/obsidian-game-dev/01-Practise-Phases/Phase-01/` —
`01-Current-Brief.md` (objective, required player loop, evidence gates,
cut order), `Level-Plans.md` (room sequence to the boss), and
`Required-Assets.md`. Read the brief before scoping any feature; it has an
explicit cut list and an explicit "do not build" list.

This file is the canonical instruction document for all coding agents here.
Shared rules, skills, and knowledge live under `.claude/`; decisions in
this file override generic guidance there.

> [!IMPORTANT]
> The ADRs under `.claude/knowledge/decisions/` and parts of
> `.claude/rules/doctrine.md` and `.claude/skills/{wrap-up,refactor}/` were
> carried over from a prior project — "Solar Defense" / `GodotWildJam-96`,
> an arcade space-action game — and still name it, its assemblies
> (`GodotWildJam96.*`), and its entities (`Squid`, `Sun`). Apply their
> reasoning where project-agnostic; ignore the Solar-Defense nouns.
> `.claude/agents/godot-advisor.md` and the `.codex/` files have been
> retargeted to this project.

## Versions (do not assume newer)

- Godot **4.7** (`config/features` in `project.godot`), pinned at 4.7.1 —
  ADR-004.
- `Godot.NET.Sdk/4.7.1` on the View project; `TargetFramework net8.0`
  (ADR-003), with an unused `net9.0` override for
  `GodotTargetPlatform=android` — no `export_presets.cfg` exists yet.
- `EscapefromUSSParkov.Sim` and `.Tests` set `<Nullable>enable</Nullable>`
  and `<ImplicitUsings>enable</ImplicitUsings>`; the View project
  (`EscapefromUSSParkov.View.csproj`) sets `<ImplicitUsings>enable</ImplicitUsings>`
  but not `<Nullable>`. New Sim/test code is nullable-aware; new View code
  is not nullable-annotated. Don't "fix" one side to match the other.
- Analyzers and warning enforcement are wired repo-wide in
  [`Directory.Build.props`](Directory.Build.props): `Roslynator.Analyzers`,
  `SonarAnalyzer.CSharp`, `Meziantou.Analyzer`, plus
  `TreatWarningsAsErrors`, `AnalysisLevel=latest`, and
  `EnforceCodeStyleInBuild` — per ADR-006. A warning fails the build. New
  suppressions go in `.editorconfig` with a stated Godot-specific reason,
  matching the ones there (S125, S1075, IDE0044/0060/0130, RCS1163/1169,
  MA0004/0011/0046, CA1825/1859/1861). `ErrorProne.NET.CoreAnalyzers`
  (also in ADR-006's set) is intentionally omitted — it ships beta-only.

## Build

```
dotnet build "Escape from USS Parkov.sln"
```

```
dotnet test "EscapefromUSSParkov.Tests/EscapefromUSSParkov.Tests.csproj"
```

`EscapefromUSSParkov.Tests` is plain xUnit (xunit 2.9.2) referencing
**only** `EscapefromUSSParkov.Sim` — not the View project.
`SimBoundaryTests.cs` asserts no Godot assembly is referenced by Sim, so a
leak fails the build. Test the pure-C# rule seam directly; don't test a
bridge script (ADR-007, ADR-009).

The bridge layer — scene tree, `GD.*`, `Input.*`, animation, audio — has
no automated coverage and is verified by playing it. Verification bar:
clean `dotnet build`, `dotnet test` green, then a manual play pass in the
editor.

## Architecture

- **Most gameplay lives in Node scripts.** That is correct for this phase.
  The brief calls for engine-lifecycle, input/physics, signals, resources,
  scene composition, animation, UI, and level flow — all View-side work.
- **One pure-C# rule seam** (ADR-009 in spirit, scoped down by the brief).
  Exactly one small rule — "preferably accuracy/spread or damage" — is
  extracted into `EscapefromUSSParkov.Sim`, a plain `net8.0` library with
  **no Godot reference at all**, and covered by tests in
  `EscapefromUSSParkov.Tests`. One-way references: View → Sim, Tests → Sim
  only. `using Godot;` in Sim is a build failure. Do **not** grow this
  into a project-wide sim-view migration — the earned benefit here is one
  tested rule and the compiler-enforced boundary around it, nothing wider.
- Sim is Godot-*namespace*-free, not just `GD.*`-free. If the seam needs
  vector math, use `System.Numerics.Vector2` — and note `Normalized()`
  must return `Zero` for a zero vector where `Vector2.Normalize` returns
  `NaN` (reimplement in a `Sim/SimMath.cs` helper if used). Convert
  vector types in one place View-side, never a hand-rolled
  `new Vector2(v.X, v.Y)` at a call site.
- Reactive / event-driven core loop: `title → play → escape or death →
  result → restart/title`. Node scripts run at Godot's own `_Process` /
  `_PhysicsProcess` cadence. There is **no** separate simulation clock.
- Enemy behaviour is intentionally primitive — direct room-scale chase, a
  trigger, a range rule, or a fixed path. **Not** `NavigationAgent2D`,
  navigation regions, view-cone perception, avoidance, or patrol
  architecture (brief's explicit exclusions).
- Where a Node bridge does call the seam, it holds no second copy of the
  rule's state: pass inputs in, use the result, don't cache it.
- Performance per `.claude/rules/performance.md`: free habits (no hot-path
  allocation, plain loops over LINQ in repeated code, bounded loops, one
  writer per field) by default; `Span<T>` / pooling / `ref`/`in` gated on
  a profile. Assumed floor: integrated GPU, 8 GB-class RAM.

## Conventions

- Class placement: `.View` by default (this is a Node-script codebase).
  The rule seam — and only genuinely pure, engine-free rule code with a
  reason to be tested — goes in `.Sim`. Anything needing
  `Node`/`Resource`/`GD.*`/`Input.*` is `.View`, always.
- One flat, file-scoped namespace per assembly: `EscapefromUSSParkov.View`,
  `EscapefromUSSParkov.Sim`, `EscapefromUSSParkov.Tests`. Folders add no
  namespace segment; `IDE0130` is off.
- Node references are `[Export]` fields wired in the editor
  (`node_paths=PackedStringArray(...)` in the `.tscn`). No `GetNode<T>()`
  or `%UniqueName` (ADR-002).
- Project-wide events are C# `event Action<T>`, not `[Signal]` (ADR-001).
  Intended shape: one `EventBus` autoload — `static Instance`, one
  `event Action<T>` per event, `EmitOnX` helpers. Pair every `+=` with a
  `-=` in `_ExitTree`.
- Entity variants share a C# base class with `protected virtual` hooks,
  not scene composition. `sealed` by default on leaf classes; unseal only
  a class that has a real subclass.
- Public members are PascalCase properties (`[Export]` where
  editor-wired); private members are `_camelCase` fields; no raw public
  fields. `protected` members on a subclassable base use the PascalCase
  property style — they're the subclass-facing API.
- Small state/mode values are enums, not magic ints.
- Save data is a `Resource` subclass with `[Export]` fields (Godot's
  native `ResourceSaver`/`ResourceLoader` format); a non-`[Export]` field
  is invisible to serialization and reverts silently on load. Saves live
  under `user://`.
- Style: Allman braces, 4-space indent, LF, 120-char lines, explicit types
  except where `var`'s type is apparent (per `.editorconfig`).
- Learning codebase: keep comments that explain *why*, named Game
  Programming Patterns, and planned-work pointers — they're part of the
  exercise, not cleanup targets (priorities.md #3). The patterns this
  phase practices are State, Observer, and Command (brief's reading list).
- Process gates from the brief: one active owner per shared `.tscn` file;
  a warning-free accepted build and clean Godot debugger output before
  work is accepted.

## Out of scope

- **Networking = Solo.** No multiplayer, authority model, prediction, or
  rollback. Don't introduce multiplayer or Fix64 guidance (ADR-005).
- **Determinism = No.** Float math is fine everywhere. No fixed-point, no
  lockstep, no replay hashing.
- **Prototype, one-and-done.** Smallest thing that works; no abstraction
  layers for a codebase this short-lived; no save-schema migration layer.
- **Local saves only.** The only outside input is the game's own
  `user://` save files — handle a missing/corrupt save (fail clean, don't
  half-load); skip input-validation doctrine for networked or
  user-content systems. No other trust boundary.
- **No modding, no localization, no platform integration.** No mod
  loading or parity stamps; strings authored in English, no
  `TranslationServer`; no Steamworks/EOS/console SDK, achievements, or
  cloud sync.
- **No DI or FSM package.** Hand-written DI (`EventBus`) and hand-written
  state machines. No Chickensoft tooling in either `.csproj`.
- **Brief's "do not build" list.** No full logistics inventory, object
  pooling, homing, `NavigationAgent2D`, navigation regions, terrain
  generation, procedural levels, skill trees, or Pulang Damit systems.
  Extra weapons, health tuning, fog of war, and the Quasimorph/Barotrauma
  art direction are stretch work only after the required evidence passes.

## Where the rules live

- `.claude/rules/doctrine.md` — three-level performance model; when
  lockstep/Fix64/host-authority apply; why a sim-view split is normally
  earned rather than default (here it's scoped to one rule seam — ADR-009).
- `.claude/rules/priorities.md` — tiebreaker: correctness > simplicity >
  documented intent > readability > performance.
- `.claude/rules/godot-csharp-conventions.md` — per-axis Godot C# idiom
  choices (Export vs `GetNode`, `event` vs `[Signal]`, inheritance vs
  composition, namespaces, save data, pausing, state machines).
- `.claude/rules/performance.md` — free habits vs. complexity-adding
  tools, plus the Godot hot-path list.
- `.claude/rules/skill-authoring.md` — conventions for `.claude/skills/*`.
- `.claude/knowledge/decisions/` — ADRs 001–009 (written for the prior
  project; the version pins, `event`-over-`[Signal]`, `[Export]`-over-
  `GetNode`, NASA Power-of-Ten adaptation, and the test harness carry over
  — the names, entities, and the *scale* of ADR-009's split do not).
- `.claude/knowledge/{godot-csharp-gotchas,multithreading-csharp-godot,gaming-patterns-index}.md`
  — C# interop pitfalls, when threading is justified, problem → pattern
  index.

## Instruction-file precedence

This file is authority. [`AGENTS.md`](AGENTS.md) is a compatibility shim
that defers here; `.codex/` mechanics and everything under `.claude/`
point back to this file. Where any of them conflicts with a line here,
this file wins.

<!-- godot-init: 2026-09-01 | genre=top-down-action net=solo det=no dim=2d
     hw=low-end exports=desktop(assumed) team=small(2) ambition=prototype
     live=one-and-done persist=local-only mod=none platform=none l10n=none
     tooling=xunit+analyzers | notes=greenfield code; design authority in
     obsidian vault 01-Practise-Phases/Phase-01; .claude/ ADRs inherited
     from GodotWildJam-96; godot-advisor + .codex retargeted; prior
     CLAUDE.md replaced; analyzers wired via Directory.Build.props -->
