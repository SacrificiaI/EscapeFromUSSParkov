# ADR-006: NASA/JPL "Power of Ten" safety-critical rules, adapted per-rule for Godot C#

## Status

Accepted.

## Context

ADR-005 rejected the ASP.NET-flavored derivatives of NASA doctrine
(`mission-critical-csharp-aspnet.md`, `nasa-npr7150-aspnet.md`) wholesale,
since both assume a networked, regulated, multi-user backend this project
doesn't have. A follow-up question: those two documents are themselves
adaptations of an older primary source — does the *original* source hold up
better, given that a single-player game process (no redundancy, no graceful
degradation, one instance, one player) shares more structural similarity
with a spacecraft's one-shot control loop than either does with a
horizontally-scaled web API?

The primary source is Gerard J. Holzmann (NASA/JPL), ["The Power of
Ten — Rules for Developing Safety-Critical
Code"](https://spinroot.com/gerard/pdf/P10exp.pdf), IEEE Computer, June 2006 —
later adopted as the
[JPL Institutional C Coding Standard](https://yurichev.com/mirrors/C/JPL_Coding_Standard_C.pdf).
The table below is the full rule-by-rule evaluation against this project's
actual premises.

## Decision

Evaluate each of the ten rules against this project's actual premises rather
than blanket-adopting or blanket-rejecting the set:

| # | Rule (paraphrased) | Verdict | Action taken |
|---|---|---|---|
| 1 | Simple control flow / no unbounded recursion | Adapt (hot-path only) | [performance.md](../../rules/performance.md) — no unbounded recursion in code called from `_Process`/`_PhysicsProcess` |
| 2 | Fixed loop bounds | **Adopt** | [performance.md](../../rules/performance.md) — new "bounded loops" free habit |
| 3 | No dynamic allocation after init | Adapt (already scoped to hot path) | No rule change — rationale reinforced (GC pause ≈ frame hitch, not "OOM") |
| 4 | Function length (~60 lines, "one screen") | Adopt | Confirmed existing practice (already noted in ADR-005) |
| 5 | 2 assertions/function average | Adapt (tool, not quota) | No change — `Debug.Assert` stays opportunistic, no numeric target imposed |
| 6 | Minimal variable scope | Adopt | Already idiomatic C#, no new rule needed |
| 7 | Validate every param/return value | **Reject** | No change — no untrusted input boundary exists in this project yet |
| 8 | Restricted preprocessor use | N/A | C#'s `#if`/`#region` isn't a macro preprocessor; nothing to restrict |
| 9 | Restricted pointer dereference | N/A | Zero `unsafe` code in this project; revisit only if that changes |
| 10 | Zero-warning compilation | **Adopt** | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<AnalysisLevel>latest</AnalysisLevel>`, and `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` added to `GodotWildJam-96.csproj`, along with the six analyzer packages (Roslynator.Analyzers, Roslynator.CodeFixes, Roslynator.Refactorings, SonarAnalyzer.CSharp, Meziantou.Analyzer, ErrorProne.NET.CoreAnalyzers), confirmed with a clean rebuild (0 warnings, 0 errors) |

Rule 10 is the strongest match: static analysis without an enforcement gate
is optional by construction — a new warning can sit ignored indefinitely.
Adopting the analyzer set and `TreatWarningsAsErrors` together closes that
gap. Doing so surfaced 69 pre-existing warnings-turned-errors on the first
build; each was either fixed for real (missing namespace on 9 files, a
redundant override, an unused parameter, a redundant `return`, 16 public
fields converted to properties with matching `.tscn` export-key updates)
or suppressed in `.editorconfig` with a stated Godot-specific reason (S125
for intentionally-kept disabled debug scaffolding, S1075 for Godot's
`res://` virtual paths being mistaken for OS absolute paths).

Rule 7 is the sharpest rejection: it exists because flight hardware can hand
back genuinely corrupted values (radiation-flipped bits, failing sensors)
and there's no recourse but defensive validation everywhere. This project's
internal calls (`Player` → `EnemyBase` → `SignalHub`) are all trusted,
same-codebase calls with no adversarial or noisy channel — validating them
defensively would be dead code by construction, which
[priorities.md](../../rules/priorities.md) already rejects.

## Consequences

### Positive

- `TreatWarningsAsErrors` now enforces the analyzer suite the project
  already pays the build-time cost for.
- Two new free habits (bounded loops, hot-path recursion depth) cost nothing
  to apply from here forward and close a real hang-risk category that
  wasn't previously named anywhere in the rule set.
- The evaluation is falsifiable and revisitable rule-by-rule — a future
  session doesn't have to re-litigate all ten at once if only one premise
  changes (e.g. a save file appears, which would revisit rule 7).

### Negative

- `TreatWarningsAsErrors` means any future analyzer upgrade that introduces
  a new default-enabled warning can break the build until addressed. This is
  the intended tradeoff (rule 10's entire point), but worth knowing going in.

### Mitigations

- If an analyzer upgrade ever produces a wave of new warnings, the fix is to
  triage and address them (or explicitly suppress with a stated reason), not
  to revert `TreatWarningsAsErrors` — reverting defeats the reason it was
  added.
