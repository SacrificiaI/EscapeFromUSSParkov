# ADR-007: Unit test harness added, scoped to engine-free logic only

## Status

Accepted.

## Context

Solar Defense had no test framework and a verification bar of "clean
`dotnet build` + manual play pass." The user asked for a refactor toward an
exemplar, SOLID/DRY teaching codebase. [doctrine.md](../../rules/doctrine.md)
warns against retrofitting a working, shipped game without a regression net,
and every candidate for refactoring is currently an instance method on a
`Node2D`/`CharacterBody2D` that mutates node state directly — none of it is
reachable by a test as written.

Three options were considered for a test framework:

- **GUT** (Gut Unit Test) — the most established Godot test addon, but
  GDScript-first; C# support is unofficial and secondary.
- **GoDotTest** — a Chickensoft testing package. This project's
  `CLAUDE.md` already explicitly declines Chickensoft tooling ("Hand-written
  DI and hand-written state machines are the convention... Don't add a DI or
  FSM package") — pulling in a Chickensoft test package contradicts that
  standing decision.
- **xUnit**, run as a plain .NET test project referencing the Game assembly
  directly, with no Godot-specific test runner at all.

The deciding technical question: can any of this project's logic run without
booting the Godot engine? Investigation confirmed yes —
`GodotSharp.dll` (`~/.nuget/packages/godotsharp/4.7.1/lib/net8.0/`) is a
plain managed .NET library containing both pure managed types (`Vector2`,
`Mathf`, `Transform2D`, usable with zero engine boot) and a P/Invoke bridge
to the native engine (`GD.Randf()`, `GD.RandRange()`, all
`Node`/scene-tree operations, which require the native runtime and cannot
run in a plain test host). This makes a normal `dotnet test` host viable for
any logic written against the pure-managed subset.

The user explicitly scoped this pass via direct answers, overriding the
plan's original default suggestions:

- Extracted pure logic stays in `GodotWildJam96.Game` — tests reference the
  Game assembly directly. `GodotWildJam96.Sim` (an existing empty,
  Godot-reference-free class library scaffold with zero hand-written source)
  is explicitly **left untouched** for this pass.
- This pass ships the harness and a written refactor roadmap only — **no
  gameplay code changes**.

## Decision

**Add `GodotWildJam96.Tests`, a plain xUnit project, to the solution.** It
references `GodotWildJam96.Game/GodotWildJam-96.csproj` directly and tests
only the subset of the Game assembly that doesn't touch `GD.*` or the scene
tree. All three required packages (`xunit` 2.9.3, `xunit.runner.visualstudio`
3.1.5, `Microsoft.NET.Test.Sdk` 17.14.1) are confirmed present in the local
NuGet cache, so restore works fully offline.

The six analyzer packages already applied to the Game project (Roslynator,
SonarAnalyzer.CSharp, Meziantou.Analyzer, ErrorProne.NET.CoreAnalyzers — see
[ADR-006](006-nasa-power-of-ten-adapted-for-godot-csharp.md)) are
**deliberately not added** to the Tests project: they're `PrivateAssets=all`
on the Game project so they don't flow transitively, and putting test code
under `TreatWarningsAsErrors` from day one adds friction with no matching
safety benefit.

`GodotWildJam96.Sim` stays empty. Nothing in this pass needed threading,
headless unit tests outside the engine's assembly graph, or networking — the
three justifications [doctrine.md](../../rules/doctrine.md) names as earning
a sim-view split — and the Game assembly is directly testable for the
engine-free subset without one.

The harness this pass ships (`HarnessTests.cs`) exists to prove the premise
above, not to cover behavior that isn't extractable yet:

1. A managed-types-without-engine-boot probe (`Vector2.FromAngle`,
   `Mathf.Tau`) — the load-bearing claim the entire roadmap rests on.
2. A cross-assembly load proof against `GameConstants.GroupPlayer`/
   `GroupSuns` — also a real runtime contract check between `AddToGroup`
   calls and `GetNodesInGroup` lookups elsewhere in the codebase, where a
   typo currently fails silently at runtime rather than at compile time.
3. A documented testable/untestable boundary probe: `typeof(Spawner)` loads
   and its base type is `Node2D`, but it is never instantiated — that would
   require the native runtime. This marks exactly where the line sits and is
   why [the refactor roadmap](../refactor-roadmap.md) extracts pure logic
   out of node scripts rather than attempting to test nodes in place.

See [refactor-roadmap.md](../refactor-roadmap.md) for the sequenced plan
this harness unblocks.

## Consequences

### Positive

- Establishes a real, offline-restorable regression net before any gameplay
  refactor touches shipped, working code.
- Confirms with an actual test (not just documentation) that `Vector2`/
  `Mathf` work outside the engine — the premise the whole roadmap depends
  on is now falsifiable, not assumed.
- Stays consistent with existing project decisions: no Chickensoft tooling
  (GoDotTest declined for the same reason `CLAUDE.md` already declines
  DI/FSM packages), no `Sim` project touched without a concrete need.

### Negative

- `GodotWildJam96.Game/project.godot` sets
  `[dotnet] project/solution_directory="../"`, meaning the Godot editor
  builds the **entire solution**, not just the Game project. Adding
  `GodotWildJam96.Tests` to `GodotWildJam-96.sln` means a test compile
  error can now break the in-editor build and stop the game from launching
  — a coupling that didn't exist before this project had a second
  buildable C# project of its own.
- The harness currently covers only the premise, not any real gameplay
  logic — it provides no protection yet for the actual refactors the
  roadmap proposes until those extractions land with their own
  characterization tests.

### Mitigations

- If the solution-build coupling proves annoying in practice, the fix is to
  drop `GodotWildJam96.Tests` from `GodotWildJam-96.sln` and run
  `dotnet test GodotWildJam96.Tests/` directly instead — this reverses with
  no code changes, since the test project's `ProjectReference` doesn't
  depend on solution membership.
- The roadmap's own sequencing (Tier 1 mechanical fixes first, Tier 2's
  `Player.FindClosestSun` as the first real extraction, Tier 3's explicit
  warning that the initial `Spawner` extraction step is unprotected until
  characterization tests exist) is the plan for closing the second
  negative — this ADR records the harness, not the completed coverage.