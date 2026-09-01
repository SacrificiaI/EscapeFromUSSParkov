---
name: godot-init
description: >
  Initialize Claude Code for a Godot 4.x C# project: detect the existing
  project and its conventions, ask an ordered architecture questionnaire,
  derive an architecture from the answers, write a customized CLAUDE.md, and
  verify the result against a real build. Use when setting up a new Godot C#
  project, or when adopting this rule/knowledge set into an existing one.
argument-hint: 'Path to the Godot project root (defaults to the current directory)'
user-invocable: true
---

# /godot-init

**Portable, project-agnostic skill** — self-contained; works for anyone who
has only this file.

## What This Does

Runs against an existing Godot 4.x C# project and produces one artifact: a
`CLAUDE.md` at that project's root, written from what the project actually
is plus what its owner decides it will become.

Five steps, in order:

0. **Detect** — read the engine version, SDK version, target framework, and
   project shape off disk. On a project that already has code, scan it for
   the conventions it already follows.
1. **Ask** — the ordered questionnaire (Section 1), with detected values
   pre-filled as defaults to confirm rather than type.
2. **Derive** — turn the answers into architecture recommendations
   (Section 2). Never asked directly.
3. **Generate** — compose and write `CLAUDE.md` (Section 3).
4. **Verify** — check every claim in the generated file against disk, run
   the build, and report (Section 4).

**Non-goals.** This skill does not create a Godot project, scaffold
folders/scenes/scripts, write `.claude/rules/` or `.claude/knowledge/`,
modify `project.godot` or any `.csproj`, or repair a failing build. It
writes one file and reports.

Self-contained by design: every question, rule, template, and cited fact
below is stated in full inline (or in the sibling `templates/` file this
skill folder ships with). Nothing here depends on a file, wiki, or memory
system outside this one — it has to work identically for anyone who has
this file, not just on the machine it was written on.

Built open, not biased toward any one team's typical project shape: every
question below is equal-weight, with real options, not a rubber-stamped
default.

**Fixed exception #1:** scripting language is always C#. GDScript is never
offered as an option. This is settled tooling policy, not a per-project
creative choice — unlike genre, networking model, and the rest, which stay
genuinely open.

**Fixed exception #2:** PVE only, never competitive PVP. Not asked as a
live question — competitive multiplayer changes the trust model completely
(server-authoritative state, hidden-info-between-players design, and
anti-cheat all become real, non-optional costs the moment any player might
be adversarial instead of cooperative), and this tool's target projects are
PVE by policy, so that branch is skipped rather than asked and then always
answered the same way.

**On "mandatory QOL":** a few items below (late-join, mod parity) are
flagged as *required by default* rather than posed as neutral toggles. That
is not the same claim as "all QOL is unconditionally mandatory regardless of
context" — late-join has nothing to attach to in a solo single-player game,
so it can't be mandatory there. The actual rule: wherever the precondition
for a QOL feature genuinely holds, don't present it as a 50/50 choice with
equal-weight options — default it on, and require a stated reason to turn it
off. Item 16 is one place this same treatment hasn't been applied yet.

---

## 0. Detect the Project

Runs before any question is asked. Everything here is read-only.

### 0.1 Locate the project root

Resolve the root to the directory containing `project.godot`: the path
given as the skill argument, else the current directory, else the nearest
parent that has one.

- **No `project.godot` anywhere** → stop. Report: this skill configures an
  existing Godot project; it does not create one. Create the project in the
  Godot editor first, then re-run.
- **`project.godot` present but `config_version` is not `5`** → stop. `5` is
  the Godot 4.x project format; a lower number is a Godot 3.x project, which
  this skill does not cover.
- **A `CLAUDE.md` already exists at that root** → confirm the target with
  the user before going further. Getting the root wrong and overwriting an
  unrelated project's instructions is the one unrecoverable failure here.

### 0.2 Read `project.godot`

Read the file directly — it is small, INI-shaped, and every value below is
a plain line match.

| Line | Yields |
|---|---|
| `config/name="…"` | Project name → the `#` heading of the generated file |
| `config/features=PackedStringArray("4.6", "C#", …)` | Engine **major.minor**, C# enabled, renderer tag |
| `run/main_scene=` | Entry scene (`uid://` or `res://`) |
| `[dotnet] project/assembly_name=` | Assembly name |
| `[autoload]` entries | Existing global services |
| `[display] window/size/viewport_*`, `window/stretch/mode` | Base resolution and stretch policy |
| `[rendering] renderer/rendering_method` | `forward_plus` / `mobile` / `gl_compatibility` |
| `[input]` action block | Existing input actions; joypad events present or not |
| `internationalization/locale/…` | Translation setup, if any |

**`config/features` carries only major.minor — never the patch.** A project
on 4.6.3 says `"4.6"`. The patch digit comes from the `.csproj` SDK line
(0.3) or from the user. Never state a patch version in the generated file
that no file on disk contains.

The renderer tag in `config/features` mirrors `renderer/rendering_method`;
`"Double Precision"` and `"C#"` are separate feature tags in the same array.
Absence of `"C#"` means the project has no C# assembly configured yet — note
it and expect 0.3 to find no `.csproj`.

### 0.3 Read the C# project files

Glob `*.sln` and `**/*.csproj` at the root (exclude `addons/`). Read each
`.csproj` found.

| Pattern | Yields |
|---|---|
| `Godot\.NET\.Sdk/([0-9]+\.[0-9]+\.[0-9]+)` | SDK version — the only patch-level signal on disk |
| `<TargetFramework>` (capture **every** occurrence with its `Condition`) | TFM, plus per-platform overrides |
| `<Nullable>`, `<LangVersion>`, `<AnalysisLevel>` | Language policy, or their absence |
| `<TreatWarningsAsErrors>`, `<EnforceCodeStyleInBuild>` | Warning policy |
| `<RootNamespace>`, `<EnableDynamicLoading>` | Namespace root, hot-reload posture |
| `<PackageReference Include="…" Version="…">` | Analyzers, test frameworks, Chickensoft packages |

The SDK version tracks the engine version but is **not** proof of which
editor build is installed. State it as the SDK version, and let the user
confirm the editor patch during the questionnaire rather than asserting it.

Also check, and record only if present: `global.json` (pinned .NET SDK),
`Directory.Build.props`, `nuget.config`, `.editorconfig`, `.gitignore`.

Run `dotnet --version` (read-only) to record the installed SDK. A failure
here means `dotnet` is off PATH — record that; Section 4 needs to know.

### 0.4 Classify greenfield vs existing

Count, excluding `.godot/`, `**/bin/`, `**/obj/`, `addons/`, and any
`*.generated.cs`:

- hand-written `.cs` files
- `.tscn` files
- whether `.claude/rules/*.md` exists

**Godot's C# source generators emit `*.generated.cs` under `obj/`** holding
`MethodName`/`PropertyName` caches and partial-class plumbing. Counting them
inflates every convention metric in 0.5 by an order of magnitude. Exclude
them explicitly; if a search tool's ignore-file handling doesn't cover it,
verify by checking that no hit path contains `obj/`, `bin/`, `.godot/`, or
`addons/`, and discard any that do.

- **Greenfield** — 0 hand-written `.cs` files, **or** ≤3 `.cs` and ≤2
  `.tscn` and no `.claude/rules/`. Skip 0.5; the questionnaire answers
  alone decide the conventions.
- **Existing** — anything else. Run 0.5.

### 0.5 Convention scan (existing projects only)

Godot's C# decision space forks where an ASP.NET project has one blessed
answer: node references, signals, entity variants, and namespaces each have
two idiomatic, mutually exclusive options that a codebase picks and sticks
with. Inferring the wrong one and writing it into a permanent instruction
file is worse than asking.

**If `.claude/rules/*.md` already exists, read all of it first.** Those
files are authority. The scan then runs in verify mode: compare what the
rules say against what the code does, and report drift as a finding. Do not
rewrite the rules, and do not let the scan override them in the generated
file.

**Confidence bar — applies to every axis.** Compute `dominant / total` for
the axis.

- **Settled** — total hits ≥ 5 **and** dominant ≥ 80%. Write it into
  `CLAUDE.md` as the convention.
- **Leaning** — dominant ≥ 60%, or total hits between 2 and 4. Surface it as
  a pre-filled default for the user to confirm; write it only if confirmed.
- **Unsettled** — neither. Omit the line entirely. A guessed convention in a
  permanent instruction file is worse than a missing one.

Put the hit counts in the **report** (Section 4.3), not in `CLAUDE.md`. The
generated file states the rule; the report states the evidence.

#### Axis A — node references: `[Export]` fields vs `GetNode<T>()`

| Search (in these files) | Side |
|---|---|
| `node_paths=PackedStringArray\(` (`**/*.tscn`) | `[Export]` — strongest signal; this line only exists for editor-wired exported node references |
| `\[Export` (`**/*.cs`) | `[Export]` — weak alone; also counts tuning values and `PackedScene` fields |
| `GetNode(OrNull)?\s*[<(]` (`**/*.cs`) | `GetNode` |
| `GetNode\w*\s*[<(][^)]*"%` (`**/*.cs`) | `GetNode` — unique-name (`%Name`) variant, note separately |
| `\[Export[^\]]*\][\s\S]{0,120}?NodePath` (`**/*.cs`, multiline) | Third style: exported `NodePath` resolved at runtime |

Weight the `.tscn` count over the raw `[Export]` count — it is the one
pattern that cannot mean anything else. **Conclusion:** *"Node references
are `[Export]` fields wired in the editor; do not introduce
`GetNode<T>()`"* / *"Node references are `GetNode<T>()` lookups"* /
*"Node references use exported `NodePath` resolved in `_Ready`"* /
unsettled.

#### Axis B — signals: `[Signal]` vs C# `event`

| Search | Side |
|---|---|
| `\[Signal\]` (`**/*.cs`) | Godot signals |
| `EmitSignal\s*\(` (`**/*.cs`) | Godot signals |
| `\.Connect\s*\(` (`**/*.cs`) | Godot signals, code-wired |
| `^\[connection ` (`**/*.tscn`) | Godot signals, editor-wired |
| `\bevent\s+(Action\|Func\|EventHandler)` (`**/*.cs`) | C# events |

Godot **built-in** node signals (`AreaEntered`, `Timeout`, `BodyEntered`)
are subscribed with `+=` in both worlds and are not evidence either way —
this axis is only about signals the project *declares*. `EmitSignal` on a
built-in is rare enough to ignore.

Record separately whether connections are wired in code or in `.tscn`: a
project with zero `[connection` lines wires everything in code, and that is
its own instruction worth writing down.

**Conclusion:** *"Project-wide events are C# `event Action<…>`; `[Signal]`
is used only where a signal must be visible to the editor"* / *"Project
signals are `[Signal]` delegates"* / unsettled. Add the subscribe/
unsubscribe lifecycle line if `_ExitTree` appears in ≥60% of files that
subscribe.

#### Axis C — entity variants: scene composition vs C# inheritance

| Search | Side |
|---|---|
| `abstract\s+(partial\s+)?class` (`**/*.cs`) | Inheritance |
| `protected\s+(virtual\|abstract)\s` (`**/*.cs`) | Inheritance |
| `partial\s+class\s+\w+\s*:\s*\w+` (`**/*.cs`, capture matches) | Collect base names; a base that is **not** a Godot node type is inheritance |
| `^\[node name="[^"]*" instance=ExtResource\(` (`**/*.tscn`) | Composition — a root `[node` line with `instance=` and no `parent=` is an inherited scene |
| `^\[node name="[^"]*" parent="[^"]*" instance=ExtResource\(` (`**/*.tscn`) | Composition — instanced child scenes |

Classify captured base names by hand: `Node2D`, `CharacterBody2D`, `Area2D`,
`Control`, `Resource`, `Node3D` and friends are the engine's; anything else
is a project base class. Also check for files matching `*Base.cs`.

**Conclusion:** *"Entity variants share a C# base class with `protected
virtual` hooks"* / *"Entity variants are inherited or instanced scenes
composed in the editor"* / *"Both, split by …"* — and if both are heavily
present, that split is itself the convention worth writing down.

#### Axis D — namespaces

| Search | Side |
|---|---|
| `^namespace\s+[\w.]+\s*;` (`**/*.cs`, capture distinct values) | File-scoped |
| `^namespace\s+[\w.]+\s*$` and `^namespace\s+[\w.]+\s*\{` (`**/*.cs`, capture distinct values) | Block-scoped |
| `<RootNamespace>` (`*.csproj`) | Declared root |
| `IDE0130` (`.editorconfig`) | A suppression here means flat namespaces are deliberate policy, not drift |

**Conclusion:** exactly one distinct namespace across every folder → *flat,
one namespace for the project*. Distinct namespaces tracking folder paths →
*per-folder, matching directory structure*. Zero declarations → *global
namespace*. Mixed with no `IDE0130` suppression → unsettled; report the
inconsistency as a finding instead of writing a rule.

#### Secondary axes — cheap, recorded when clear

- **Class shape** — `sealed\s+partial\s+class` vs non-sealed
  `partial class`.
- **Global services** — the `[autoload]` block from 0.2, plus
  `static\s+\w+\s+Instance` (hand-rolled singletons).
- **Formatting** — `.editorconfig` keys: `indent_size`, `max_line_length`,
  `csharp_new_line_before_open_brace`, `csharp_style_var_*`,
  `csharp_style_namespace_declarations`.
- **Testing** — look for `**/*[Tt]est*.csproj`; check every `.csproj` for
  `GoDotTest|gdUnit4|GUT|xunit|NUnit|MSTest`. No match → the verification
  bar is build plus manual play, and that belongs in the generated file.
- **Loop shape** — counts of `_PhysicsProcess` and `_Process` vs
  event-handler methods. If this contradicts the core-loop shape derived in
  Section 2, **surface the contradiction to the user** rather than letting
  either side win silently.

### 0.6 Detect competing instruction files

Look for `.github/copilot-instructions.md`, `.github/instructions/*.md`,
`AGENTS.md`, `GEMINI.md`, `.cursorrules`, `.cursor/rules/*`,
`.windsurfrules`. Any hit earns a precedence section in the generated file
(Section 3.4). Read them only far enough to name them — this skill does not
audit or reconcile their contents.

### 0.7 Pre-fill map — detection → questionnaire defaults

Detection never skips a question. It sets which option is offered first.

| Detected | Pre-fills |
|---|---|
| `.tscn` root node types are `Node2D`/`CharacterBody2D`/`TileMap*` | Item 6 → 2D. `Node3D`/`CharacterBody3D` → 3D |
| `renderer/rendering_method` is `mobile` or `gl_compatibility`, or `"Mobile"` in `config/features` | Item 7 → low-end or mobile-class |
| `export_presets.cfg` present — check its `platform=` lines | Item 8 → the listed platforms |
| `MultiplayerSynchronizer`/`ENetMultiplayerPeer`/`[Rpc`/`Multiplayer.` found in code | Item 3 → not Solo |
| `.csproj` has `Chickensoft.` package references | Item 17 → those packages |
| `.csv`/`.po` translation files, or `internationalization/locale` set | Item 15 → not None |
| `[input]` block contains `InputEventJoypadButton` / `InputEventJoypadMotion` | Item 16 → gamepad included |
| `ResourceSaver`/`FileAccess.Open`/`ConfigFile` found in code | Item 12 → not none |

State every pre-fill as detected, so the user can see what it was inferred
from and correct it.

### 0.8 Asking the questionnaire

Batch the 17 items in dependency order so each conditional item only
appears once its gate is answered, roughly:

1. Genre (1), Dimensionality (6), Hardware floor (7), Export targets (8).
2. Resolution model (2) *if gated on*, Networking (3), Determinism (5).
3. *If Online:* authority model, player ceiling, session discovery (4).
4. Team size (9), Ambition (10), Live-service (11), Persistence (12).
5. Modding (13), Platform integration (14), Localization (15),
   Accessibility & input (16).
6. *If platform ≠ None:* which services; Chickensoft tooling (17).
7. Confirm the engine patch version, since `config/features` cannot supply
   it.

Items 4, 8, 14, and 16 are multi-select. Items flagged required-by-default
(late-join under item 3, mod parity under item 13) are **not asked** —
state them as adopted in the final report and note that turning one off
takes a stated reason.

---

## 1. Ordered Questionnaire

### 1. Genre

`Strategy / Simulation / Survival-Crafting / Action-Adventure / Platformer /
RPG / Puzzle / Other`

Asked first — conditions several later questions.

> Hybrid games are real, not an edge case. Factorio is Construction &
> Management Simulation at its core (automation/logistics is the actual
> loop) with a Survival-Crafting opening and a Strategy-adjacent
> base-defense layer against enemy waves. It doesn't cleanly single-tag, and
> strategy-plus-background-sim projects are exactly the ones most likely to
> blend Strategy/Simulation/Survival-Crafting. Single-select here will
> misclassify those on a fairly regular basis — multi-select is worth
> considering if that pattern turns out to be common enough to design for
> now.

### 2. Resolution model — *only if Genre is Strategy or Simulation*

`Turn-based / Real-time-with-pause / Real-time-strategy`

Architecturally load-bearing specifically for these genres: turn-based needs
simultaneous-turn conflict resolution; real-time-with-pause is the closest
fit to a host-authoritative background sim; real-time-strategy at large unit
counts is historically where lockstep/deterministic sim gets reconsidered
even when host-authoritative was the starting default.

### 3. Networking model

`Solo / Local co-op (same machine) / Online multiplayer`

→ if **Online multiplayer**: `Host-authoritative / Dedicated server /
Deterministic lockstep`

→ if **Online multiplayer**: player count ceiling — `2–4 / 4–8 / 8–16 /
16+`. Not decorative: the SimEventBus shape recommendation in Section 2
reads this value directly, and has nothing to compute from without it.

→ if **Local co-op or Online multiplayer**: drop-in/drop-out (late-join)
support is **required by default, not an asked toggle** — mandatory
wherever the precondition holds, not unconditionally (see Status). Late-join
and save/load are the same architectural problem: serialize authoritative
state, transfer it, hydrate it cleanly — a late-joining player is just
save/load with a network in between. Override only with a stated reason
(narrative pacing that breaks under late-join, for example), not by
default.

### 4. Session / lobby discovery — *only if Networking (3) is Online multiplayer*

`Direct IP / LAN / Platform lobby (Steam/EGS) / Dedicated matchmaking
service` — multi-select, not exclusive. A real game commonly supports more
than one at once: a direct-IP option stays useful for LAN play and for
privacy-conscious or DRM-free players even after a platform lobby exists as
the primary path — supporting both isn't an either/or architectural cost.

### 5. Determinism & replay requirements

`Yes / No` — explicit, not assumed. Most likely to matter when networking is
online multiplayer or genre is Strategy/Simulation, but asked regardless
since a solo game can still want verifiable replays.

### 6. Dimensionality

`2D / 2.5D / 3D`

### 7. Hardware floor

`Low-end (integrated GPU, 8GB-class RAM) / Mid–high-end desktop / Mobile-class`

### 8. Export targets

`Desktop / Mobile / Web (HTML5) / Console` — multi-select.

### 9. Team size

`Solo / Small team / Larger team`

### 10. Project ambition / lifespan

`Jam or prototype / Shipping hobby project / Long-running production`

### 11. Live-service cadence

`One-and-done release / Ongoing content updates & balance patches`

Drives whether save-schema versioning/migration gets designed in now.

### 12. Persistence & save/load

`Local only / Local + platform-native cloud (Steam Cloud / EGS — platform
owns conflict handling) / Local + custom cloud sync (you own conflict
resolution)`

### 13. Modding — target tier

Pick a tier **on purpose**, not "as moddable as possible" — each one costs
real, escalating complexity:

`None / Tier 0 — data tweaks (edit stats, names, text of existing content) /
Tier 1 — new content (units/items/abilities as new defs + art) / Tier 2 —
behaviour from data (new mechanics composed from a parametrized verb
catalog) / Tier 3 — code mods (full-trust plugin assemblies)`

→ if **any tier ≥ 0** and networking is multiplayer (or lockstep): mod
parity is a lobby-join requirement by default, still overridable — content
is a third determinism ingredient alongside seed and commands, so mismatched
mods or mod versions desync silently even with zero malicious intent. The
mechanism: stamp every save and every lobby handshake with the active mod
list and versions, compare stamps before letting a session start or a save
load, and name every mismatch explicitly (missing mod, changed version,
added mod) rather than failing silently or generically. Pure view-layer
asset reskins that never touch a def or a registry are the one modding form
that stays parity-free.

→ Tier 3 is flagged, not steered toward: full-trust, no sandbox (mod code
can do anything the game can, including file and network I/O — modern .NET
has no sandboxing story), and a real ongoing version-drift maintenance cost
between the game's assemblies and whatever mods were compiled against.
Default recommendation: build tiers 0–2 first; tier 3 only once real modders
are actually asking for it.

### 14. Platform integration surface

`None yet / Steam / Epic Games Store / Console` — multi-select, then which
services: `Achievements / Crossplay & cross-progression / IAP or DLC
entitlements`

→ if **IAP/DLC entitlements**: the same trust-boundary shape as modding
above — don't trust the client's claim of ownership, verify against the
platform's receipt API before granting anything.

### 15. Localization / i18n

`None / Single language / Multi-language from day one`

→ if **not None**: TranslationServer hooks and RTL-aware layout (containers,
not manual positioning) are recommended from day one, not retrofitted —
retrofitting hardcoded UI strings and fixed-width layouts later is the
expensive path, and text-length variance alone (German and Finnish commonly
run 30%+ longer than English) breaks layouts that were never designed to
flex.

Kept as its own question rather than folded into Accessibility (16) — it's
a distinct architectural concern (content/UI pipeline), not a subset of
input/rendering.

### 16. Accessibility & input

`Input devices: KB+M / +Gamepad / +Touch` (multi-select) — `Remapping
required: Yes / No` — `Colorblind & subtitle support: Yes / No`

**Open design question:** should this follow the same required-by-default
pattern as late-join (3) and mod parity (13), rather than a neutral toggle?
Left as an asked toggle for now — no principled reason found yet to treat it
differently from those two, but none found to treat it the same either.

### 17. Chickensoft tooling adoption

`None / AutoInject (reflection-free node DI) / LogicBlocks (hierarchical
state machines) / GoDotTest (headless test runner) / GodotEnv (Godot
install & addon manager)` — multi-select, "None" is a real answer for a
small or early-stage project, not a fallback.

Most relevant at higher team size and project ambition. A small,
early-stage, or solo project can reasonably skip all of these — hand-written
DI and a hand-written state machine are a legitimate choice at that scale,
not a gap. These packages earn their cost once a team is large enough, or a
project ambitious enough, that reflection-free DI and serializable state
machines save more than they cost to adopt and maintain.

---

## 2. Derived Recommendations (computed from answers, never asked directly)

Mirrors a `dotnet-init`-style flow that never asks "Clean Architecture or
VSA?" directly — it asks domain complexity, team size, and module
boundaries, then *recommends* an architecture. Same shape here:

- **Core loop shape** (ticking background sim vs. reactive/event-driven) —
  suggested from Genre (Strategy/Simulation → ticking; Action/Platformer/
  Puzzle → reactive) and confirmed/overridden, not asked as a separate
  question.
- **Sim-View-Bridge** — recommended when core loop is ticking, OR
  networking is multiplayer, OR (team size ≥ small AND ambition ≥ shipping
  hobby project). Otherwise: put logic directly in node scripts — this is
  architecture earned by need, not defaulted to.
- **SimEventBus shape** — canonical typed-queue implementation if player
  count ceiling (3) is 8–16 or 16+, or projected entity/event volume is
  otherwise high from Genre (1) and Hardware floor (7); the simpler boxed
  `List<ISimEvent>` version otherwise.
- **Authority / determinism approach** — host-authoritative + `float` stays
  the default even under load; Fix64/lockstep only recommended when
  Determinism = Yes **and** Networking = Online multiplayer **and**
  Authority = Deterministic lockstep. Never recommended by default.
- **Late-join / session hydration path** — recommended through the *same*
  serialize-authoritative-state → transfer → hydrate pipeline as save/load,
  not a separate system, whenever Networking (3) is Local co-op or Online
  multiplayer.
- **Localization hooks** — TranslationServer + RTL-aware container layout
  recommended from day one whenever Localization (15) is not None.
- **Trust-boundary validation emphasis** — scales with how many "yes"/
  non-None answers came out of Persistence (12), Modding (13), and Platform
  integration (14). Nothing named in any of the three → skip trust-boundary
  validation doctrine in the generated CLAUDE.md entirely — there's no
  boundary to defend.
- **Save-schema versioning/migration** — recommended when Live-service
  cadence (11) = ongoing updates, regardless of other answers.
- **DI / state-machine / testing approach** — from Chickensoft tooling (17):
  AutoInject recommended over hand-rolled autoload/export-based DI once
  selected; LogicBlocks recommended over a hand-rolled `State` pattern
  implementation; GoDotTest recommended as the testing bar in place of a
  build-and-manual-play verification baseline. None selected → fall back to
  hand-written conventions, unchanged.

---

## 3. Generate CLAUDE.md

### 3.1 What the generated file is for

A `CLAUDE.md` states **decisions and locations**, not explanations. Every
line answers "what would a competent stranger get wrong here?" Version
pins, the exact build command, the architecture that was chosen, the
conventions already in force, and — the highest-value part — what
deliberately does *not* apply.

Hard rules:

- **60–150 lines.** Past that it stops being read.
- **No tutorials, no code samples** beyond the literal build command.
- **Every factual claim traces to a file read in Section 0 or an answer
  given in Section 1.** Nothing inferred, nothing assumed, nothing
  recalled.
- **Omit, never stub.** A section whose trigger did not fire is absent. No
  "N/A", no "not applicable yet", no empty headings.
- **No `{{` placeholders left in the output.**

### 3.2 Handling an existing CLAUDE.md

If one exists, ask before touching it and offer three paths: **replace**
(the existing file is stale), **merge** (keep every existing section, add
only the ones missing, and list any factual conflict rather than resolving
it silently), or **write alongside** as `CLAUDE.generated.md` for manual
merge. Merge is the default offer.

### 3.3 The template

The literal skeleton lives in the sibling file
[`templates/CLAUDE.md.template`](templates/CLAUDE.md.template) in this same
skill folder — read it, fill every `{{PLACEHOLDER}}` from Sections 0–2, and
write the result. It travels with this file; nothing about it depends on
anything outside this skill's own folder.

The template's spine (`# {{PROJECT_NAME}}`, `## Versions`, `## Build`,
`## Architecture`, `## Conventions`, `## Out of scope`) is always emitted.
Everything past the spine is a block gated on a specific answer or
detection result, documented inline in the template itself next to each
block — emit a gated block whole, or not at all, never as an empty
heading.

One block deserves calling out here because it's the point of this whole
exercise: **`## Out of scope`** is generated from every *negative* answer
(Networking = Solo → no multiplayer/Fix64 guidance; Determinism = No → no
fixed-point/lockstep; Modding = None → no mod-parity stamps; and so on for
every other item). This is where the target project's actual reality gets
recorded, including exactly how it deviates from generic doctrine — for
example, a project whose enemy variants turn out to be instanced scenes
rather than a C# inheritance hierarchy should say so here, even if a
generic Godot rule file elsewhere describes inheritance as the default. Any
`.claude/rules/` or `.claude/knowledge/` this skill finds already in place
(0.6, Section 3.4's "Where the rules live" block) is written for *any*
Godot C# project; this generated file is the one place this specific
project's deviations from that generic doctrine get written down, so a
future session reads the deviation instead of rediscovering it.

Never silently overwrite an existing `CLAUDE.md` — see 3.2.

---

## 4. Verify and Report

### 4.1 Self-check the generated file — always runs

CLAUDE.md generation cannot break a build, so the build is not what proves
this step worked. These five checks are.

1. **Every version string in the file appears verbatim in a file on disk.**
   Re-read `project.godot` and the `.csproj` and compare each one. An
   engine patch digit that no file contains, and that the user did not
   confirm, is a defect — remove it.
2. **Every path named in the file exists.** Confirm each one on disk.
3. **The build command names a file that exists.**
4. **No `{{` remains, and no heading has an empty body.**
5. **Nothing appears in both `## Architecture` and `## Out of scope`.**
   This catches the most likely generation error: a Section 2
   recommendation emitted alongside its own negation.

Any check that fails is fixed before reporting, not reported as a caveat.

### 4.2 Build baseline — runs only if a `.sln` or `.csproj` was detected

Skip entirely when 0.3 found no C# project. This skill does not create one.

Run from the project root, with a generous timeout — the first build
restores NuGet packages, including `Godot.NET.Sdk` itself:

```
dotnet build "<solution or project file>"
```

This is a **baseline capture**, not a check on the generated file: it
confirms that the build command written into `## Build` is the correct one
and that the recorded SDK version resolves. A failure here is pre-existing.

| Result | Report |
|---|---|
| Exit 0 | Build command verified; state the elapsed result plainly. |
| `dotnet` not found on PATH | Report it. The Build section stays as written; mark the command unverified in the report, not in the file. |
| NuGet restore failure | First error line, verbatim. State that it is environment or network, not the generated file. |
| `Godot.NET.Sdk` cannot be resolved | Report it prominently — this is the one failure that undermines generated content, because the SDK version recorded in `## Versions` is the thing that failed to resolve. |
| Compile errors | Count, plus the first three `error CS` lines. State they are pre-existing and that CLAUDE.md was still written. |

**Never attempt a fix.** Repairing a broken build is a different task with
a different blast radius. Report and stop.

### 4.3 Report format

```
Detected    {{greenfield | existing}} — Godot {{version}}, SDK {{version}}, {{TFM}}
            {{N}} C# files, {{M}} scenes{{, existing .claude/rules/}}
Conventions {{axis: verdict (hits) — one line each, existing projects only}}
Architecture {{the Section 2 calls that fired, one line each}}
Defaults    {{required-by-default items adopted without being asked}}
Written     {{path}} ({{N}} lines){{, replaced | merged | written alongside}}
Build       {{passed | failed: reason | skipped: no C# project}}
Next        {{2-4 concrete steps}}
```

Report the convention-scan hit counts here — this is where the evidence
belongs, not in the generated file. Report every axis that came back
**unsettled** and was therefore omitted, so a silent omission stays
distinguishable from a scan that was never run.

Report every required-by-default item adopted without being asked, and
state plainly that turning one off takes a stated reason.

**Next steps** are drawn from what actually happened: open the project in
the Godot editor once if `.godot/` is absent; review `## Out of scope`
first, since it is the section most likely to be wrong and the most
expensive to leave wrong; re-run this skill if any questionnaire answer
changes; resolve any competing instruction file named in the precedence
block.

**Done** looks like: `CLAUDE.md` exists at the project root, contains no
unresolved placeholder, every version and path in it verified against
disk, and either a passing build or an explicitly stated reason the build
was not run.

---

## Open Questions

- **Whether item 16 (Accessibility & input) should move to the same
  "required by default" treatment as item 3's late-join clause and item
  13's mod parity clause.** Flagged in item 16 itself. No principled
  reason found yet to treat it differently from those two, and none found
  to treat it the same either.

- **IAP/entitlement validation, in detail.** The shape is settled and
  stated in item 14 and in the generated file's trust-boundary block:
  never trust the client's own ownership claim; verify against the
  platform's receipt API before granting anything. What is not written up
  is the per-platform specifics — which API, which caching posture, and
  what a verification failure should do to an already-granted
  entitlement.