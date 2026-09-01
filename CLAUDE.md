# Solar Defense

Working title recovered from the Windows export path (`SolarDefense.exe`
in `export_presets.cfg`) — the Godot project itself is still named
`GodotWildJam-96` (`config/name` in `project.godot`), submitted for Godot
Wild Jam 96. 2D top-down arcade space-action: the player pilots a ship
around a starfield, siphoning energy from suns to keep a 6-level energy
pool charged while dodging and shooting `Squid` and `Devourer` enemies —
hitting 0 energy levels ends the run (`GameOverScreen.tscn`). Single-player,
no networking, built by a 2-contributor jam team. Optimize for finishing a
short-lived codebase, not for scaling one.

This file is the single canonical instruction document for Claude Code,
Codex, and other coding agents working in this repository. Shared rules,
skills, agent playbooks, and knowledge remain under `.claude/`; do not create
parallel copies for another agent runtime.

## Shared agent integration

Project-specific decisions in this file override generic guidance in
`.claude/`. In particular, this project's small, single-player jam scope and
project-wide sim-view split take precedence over advice written for larger,
networked, or long-lived games.

### Runtime-specific integration

- Claude Code uses the existing `.claude` settings, agent, skills, and
  workflow wiring directly.
- Codex uses `.codex/config.toml` to discover this file and
  `.codex/agents/godot-advisor.toml` to register the one project specialist.
  Those files contain Codex mechanics only and point back to the canonical
  Markdown under `.claude/`.
- Claude-only frontmatter, tool names, model settings, hooks, memory behavior,
  and slash-command registration are integration metadata. Other runtimes
  should use their native equivalents while following the shared guidance.
- Root `AGENTS.md` is a small compatibility shim containing shared operating
  principles and directing agents back to this canonical file. Keep `.agents/`
  absent; do not translate `.claude` content into a parallel runtime-specific
  copy.

### Agent routing

| Agent | Canonical playbook | Use for |
|---|---|---|
| `godot-advisor` | `.claude/agents/godot-advisor.md` | Non-trivial gameplay design, Godot/C# architecture, code review, pattern selection, and implementation or refactoring advice |

Use the specialist when its domain matches the task; otherwise handle the
task directly. The specialist reads this file first, then its playbook and
only the shared skills or references needed for the request.

### Skill loading

Load shared guidance in this order:

1. Read `CLAUDE.md` completely.
2. Read `.claude/agents/godot-advisor.md` when the specialist domain applies.
3. Read the matching `.claude/skills/<name>/SKILL.md` and only the rules or
   knowledge files it directly requires.

| Task | Shared skill |
|---|---|
| Initialize or adopt these conventions in a Godot C# project | `godot-init` |
| Choose the least sophisticated applicable game pattern | `pattern-check` |
| Make a small project-convention refactor | `refactor` |
| Carry out an end-to-end sim-view migration | `refactor-sim-view` |
| Verify a technical document against current official sources | `verify-doc` |
| Record reusable learning-project lessons | `lesson-maker` |
| Split the working tree into atomic commits | `commit` |
| Save progress before continuing | `checkpoint` |
| End or resume a session through the handoff lifecycle | `wrap-up` |

Claude Code may expose these as slash commands. Other runtimes should treat
the names as shared workflows and load the same `.claude/skills/` source.

### Roslyn and source navigation

Prefer the available Roslyn MCP server for semantic C# operations such as
symbol definitions, references, implementations, diagnostics, project
relationships, call chains, and overrides. Use `rg`, focused source reads,
and normal `dotnet` commands when the semantic operation is unavailable or
plain text search is the better fit.

### Conflict and response defaults

- Resolve conflicts in this order: correctness, simplicity, documented
  project intent, readability, then measured performance. `CLAUDE.md` is the
  final authority for project-specific choices.
- For a small task, load only the directly relevant skill or source files.
  For architecture or cross-system work, inspect the affected project graph
  and established decisions before proposing changes.
- Lead with the recommended outcome, keep implementation ahead of extended
  explanation, and state concrete tradeoffs or scope triggers when they
  matter.

## Versions (do not assume newer)

- Godot **4.7** (`config/features` in `project.godot`), pinned at 4.7.1 per
  `.claude/knowledge/decisions/004-godot-version-pin-4.7.1.md`.
- `Godot.NET.Sdk/4.7.1`, `TargetFramework net8.0`, with an unused
  `net9.0` override for `GodotTargetPlatform=android` (no Android export
  preset exists in `export_presets.cfg`).
- No `<Nullable>` or `<LangVersion>` set in the Game project — nullable
  reference types are off by default there. `GodotWildJam96.Sim` has
  `<Nullable>enable</Nullable>` and now holds the whole simulation layer, so
  new Sim code is written nullable-aware; new Game code is not. Don't
  "fix" one side to match the other.
- `TreatWarningsAsErrors`, `AnalysisLevel=latest`, and
  `EnforceCodeStyleInBuild` are all on, backed by six analyzer packages
  (Roslynator, SonarAnalyzer.CSharp, Meziantou.Analyzer,
  ErrorProne.NET.CoreAnalyzers) — see
  `.claude/knowledge/decisions/006-nasa-power-of-ten-adapted-for-godot-csharp.md`.
  A warning fails the build; any new suppression needs a stated
  Godot-specific reason in `.editorconfig`, matching the existing ones
  there (S125, S1075, IDE0044, IDE0060, MA0004/0011/0046, RCS1163/1169).

## Build

```
dotnet build "GodotWildJam-96.sln"
```

```
dotnet test "GodotWildJam96.Sim.Tests/GodotWildJam96.Sim.Tests.csproj"
```

`GodotWildJam96.Sim.Tests` is a plain xUnit project (no Godot test runner)
that references **only** `GodotWildJam96.Sim` — deliberately not the Game
project, so a Godot dependency leaking into the simulation breaks this
build. It covers the simulation classes directly; don't write a test that
drives a bridge script, because a bridge with logic worth testing is a
bridge that hasn't finished handing that logic over. See
`.claude/knowledge/decisions/007-unit-test-harness-scoped-to-pure-logic.md`
and `009-sim-view-separation.md`. Its own folders mirror Sim's categories
one class per file (`Player/`, `Enemies/`, `Suns/`, `Combat/`, `Utils/`),
flat namespace `GodotWildJam96.Sim.Tests` regardless of subfolder;
`SimBoundaryTests.cs` stays at the root since it tests the assembly
boundary itself, not one entity category.

The bridge layer itself — scene tree, `GD.*`, `Input.*`, animation and
audio — still has no automated coverage and is verified by playing it.
Verification bar: a clean `dotnet build`, `dotnet test` green, then a
manual play pass in the editor.

## Architecture

- **Sim-view split, project-wide.** Gameplay rules live in
  `GodotWildJam96.Sim`, a plain `net8.0` class library with **no Godot
  reference at all**; node scripts under `GodotWildJam96.Game` are bridges
  that poll input, call into the simulation, and render what it returns.
  Dependency direction is one-way and enforced by the reference graph:
  Game → Sim, and `GodotWildJam96.Sim.Tests` references **only** Sim, so a
  `using Godot;` in the simulation layer is a build failure, not a review
  catch. See `.claude/knowledge/decisions/009-sim-view-separation.md`.
- Sim is Godot-*namespace*-free, not merely `GD.*`-free. It uses
  `System.Numerics.Vector2`, and `Sim/Utils/SimMath.cs` reimplements the Godot
  4.7 helpers whose semantics differ from the `System.Numerics`
  equivalents — most importantly `Normalized()`, which returns `Zero` for a
  zero vector where `Vector2.Normalize` returns `NaN`. Use `SimMath`, not
  `System.Numerics` directly, for anything Godot had its own helper for.
  `classes/Utils/SimVec.cs` in the Game assembly is the **only** place
  vectors convert between the two representations — don't hand-roll a
  `new Vector2(v.X, v.Y)` at a call site.
- The bridge never holds a second copy of simulation state. Where Godot
  owns the transform (`CharacterBody2D.Velocity` after `MoveAndSlide`), the
  bridge loads it into the simulation at the top of the tick and writes it
  back, rather than letting the simulation keep a copy that drifts.
- Still reactive/event-driven, not a fixed-tick background sim: 6 scripts
  override `_PhysicsProcess`/`_Process`, and each drives its simulation
  object from Godot's own cadence. There is no separate simulation clock —
  adding one is a separate decision that needs a replay/networking reason.
- One global event bus, `globals/EventBus.cs`, registered as the
  `EventBus` autoload: a self-assigned `static Instance`, one plain C#
  `event Action<T>` per event, and `EmitOnX` helpers that
  null-conditionally invoke. Zero `[Signal]` declarations, zero
  editor-wired `[connection]` blocks in any `.tscn` — all wiring is `+=`/
  `-=` in code.
- Entity variants that share behavior use C# inheritance with `protected
  virtual` hooks: `EnemyBase` → `Squid`, `Devourer`; `Sun` → `MainSun`.
  Scene composition isn't the pattern for variants here.
- No authority model, no host/client split, no fixed-point math —
  single-player, nothing to keep in sync across machines.

## Conventions

- Node references are `[Export]` fields wired in the editor — every
  `.tscn` with a wired node path uses `node_paths=PackedStringArray(...)`,
  and there are zero `GetNode<T>()`/`%UniqueName` lookups in the codebase.
  Don't introduce one.
- Two flat namespaces, one per assembly: `namespace GodotWildJam96;` in the
  Game assembly, `namespace GodotWildJam96.Sim;` in the Sim assembly. Both
  are file-scoped and flat — folders inside an assembly never add a
  namespace segment. The split is deliberate: the `using GodotWildJam96.Sim;`
  at the top of a bridge script is a visible marker of where the boundary
  is. Enforced by the analyzer suite (MA0047/S3903 fail the build on a type
  with no namespace); `.editorconfig` sets
  `dotnet_diagnostic.IDE0130.severity = none` so folders don't have to
  match — deliberate policy, not drift.
- Public members are PascalCase properties (`{ get; set; }`, `[Export]`
  where editor-wired); private members stay `_camelCase` fields. A raw
  public field fails the build (S1104). This includes `[Export]` fields
  exposed to the Inspector — see `Sun.EnergyValuebar`, `Sun.SiphonSound`,
  `Spawner.SunScene`, etc. for the pattern.
- `protected` members on a subclassable base follow the same PascalCase
  property style as `public` ones, not the private `_camelCase` field
  style — see `EnemyBase`'s `Speed`, `SunPoints`, `Lives`, `StolenPower`,
  `TargetRotation`, `TurnSpeed`. They're `EnemyBase`'s subclass-facing API
  surface (read by `Squid`/`Devourer`), not private state that happens to
  be shared, so they're styled like the public API they effectively are.
- Small state/mode values are enums, not magic ints — `SiphonDirection`
  (Out/In), `SiphonOwner` (Player/Enemy), `SquidMoveState`
  (Waiting/Thrusting/Coasting) replace what used to be 0/1/2 threaded
  through method signatures with only a comment to explain the meaning.
- Style: Allman braces, 4-space indent, LF endings, 120-char lines,
  explicit types except where `var`'s type is apparent
  (`csharp_style_var_when_type_is_apparent`).
- `sealed` by default on every leaf class, enforced project-wide. `Sun` and
  `EnemyBase` stay unsealed — they're the only two classes with a real
  subclass (`MainSun`; `Squid`, `Devourer`). Assume any other class here is
  sealed unless you're about to give it its first subclass.
- Where a new class goes is decided by the boundary first, the folder
  second. If it holds gameplay state or a gameplay rule and needs no engine
  type, it belongs in `GodotWildJam96.Sim`, categorized into PascalCase
  folders that mirror the Game assembly's own entity scenes: `Player/`
  (`ShipMotion`, `ChargeMeter`, `PlayerSiphonState`, `ExposureTimer`,
  `EnergyPool`), `Enemies/` (`EnemyVitals`, `SquidMotion`,
  `DevourerApproach`), `Suns/` (`SunEnergy`), `Combat/` (`ProjectileMotion`,
  `ShotProfile`), `Utils/` (`SimMath`, `NearestTarget`, `SpawnPlacement`),
  and `Enums/` (`SiphonDirection`, `SiphonOwner`, `SquidMoveState`) — small
  value types cross-cutting more than one entity stay grouped by kind there
  rather than split across the entity folders. Folders are purely
  organizational: the namespace stays flat regardless (see above), so a new
  Sim file only needs the right folder, not a new `using`. If it needs a
  `Node`, `Resource`, `GD.*`, or `Input.*`, it belongs in
  `GodotWildJam96.Game`.
- Inside the Game assembly, `classes/` splits into `Constants/` (pure
  constant holders like `GameConstants`), `Utils/` (stateless static
  helpers like `SimVec`), `Anims/` (`ThrusterAnimator`), and `Images/`
  (the `Resource` subtype `ScrollingBackgroundImages`). `ThrusterAnimator`
  stays on the Game side deliberately — it drives four `AnimatedSprite2D`s
  and polls `Input`, so it is view code that happens not to be a `Node`,
  not a simulation class. (Folder casing is PascalCase project-wide,
  matching the Game assembly's `scenes/Player/`, `scenes/Enemies/`, etc. —
  the same convention now applied to Sim's category folders and Game's
  `classes/` category folders alike.)

## Out of scope

- Networking = Solo. No multiplayer, no authority model, no prediction or
  rollback, no relay/transport concerns. Don't introduce multiplayer or
  fixed-point (Fix64) guidance.
- Determinism = No. Floating-point is fine everywhere. No fixed-point
  math, no lockstep, no replay hashing.
- No mod loading, no def registries, no mod-parity stamps.
- Strings are authored directly in English. No `TranslationServer` wiring
  until localization is a stated goal.
- No save-schema migration layer — this is a one-and-done jam release, not
  a live-service game.
- No Steamworks, EOS, or console SDK integration; no entitlement checks.
- No trust boundary. No save/load system exists yet either, and
  persistence, modding, and platform integration are all absent — skip
  input-validation doctrine written for networked or user-content systems.
- Prefer the smallest thing that works. Don't add abstraction layers for a
  codebase this short-lived.
- Hand-written DI and hand-written state machines are the convention (see
  `EventBus`, `protected virtual` hooks above). Don't add a DI or FSM
  package (no Chickensoft tooling is referenced in either `.csproj`).

## Where the rules live

- [`.claude/rules/doctrine.md`](.claude/rules/doctrine.md) — the
  three-level performance model, when lockstep/Fix64/host-authority
  actually apply, and why sim-view separation is earned, not default.
- [`.claude/rules/priorities.md`](.claude/rules/priorities.md) — the
  conflict tiebreaker order: correctness > simplicity > documented intent
  > readability > performance (split into free habits vs. gated tools).
- [`.claude/rules/godot-csharp-conventions.md`](.claude/rules/godot-csharp-conventions.md) —
  the per-axis idiom choices (Export vs. `GetNode`, `[Signal]` vs. `event`,
  inheritance vs. composition, namespaces) this project picked, and why.
- [`.claude/rules/performance.md`](.claude/rules/performance.md) — free
  habits vs. complexity-adding tools, plus the Godot-specific hot-path
  list.
- [`.claude/rules/skill-authoring.md`](.claude/rules/skill-authoring.md) —
  conventions for this project's own `.claude/skills/*` files.
- `.claude/knowledge/decisions/` — nine ADRs recording calls already made
  (events over `[Signal]`, `[Export]` over `GetNode`, the net8.0/Godot
  version pins, the NASA Power-of-Ten adaptation, the unit test harness,
  three judgment calls from the teaching-codebase refactor — plain C#
  collaborators over scene components, `System.Random` injection over an
  `IRandomSource` interface, and the deliberately-unfixed sun distribution
  — and **ADR-009, the project-wide sim-view migration**, which supersedes
  this file's former "no sim-view split" position and partly supersedes
  ADR-008's first decision).
- [`.claude/knowledge/godot-csharp-gotchas.md`](.claude/knowledge/godot-csharp-gotchas.md),
  [`multithreading-csharp-godot.md`](.claude/knowledge/multithreading-csharp-godot.md),
  [`gaming-patterns-index.md`](.claude/knowledge/gaming-patterns-index.md) —
  reference material: verified C# interop pitfalls, when threading is
  actually justified, and a problem → pattern index.

<!-- godot-init: 2026-08-24 | genre=other(arcade-action) net=solo det=no
     dim=2d hw=low-end exports=desktop(windows,macos,linux) team=small(2)
     ambition=jam live=one-and-done persist=none mod=none platform=none
     l10n=none input=kbm tooling=none -->
