# ADR-003: `net8.0` target and the .NET 8 EOL clock

## Status

Accepted, with a known expiration date to revisit.

## Context

`GodotWildJam-96.csproj` targets `net8.0` (`net9.0` when
`GodotTargetPlatform == android`), via `Godot.NET.Sdk/4.7.1`. This wasn't a
deliberate choice made in this project — it's the SDK's default for a
project created against Godot 4.4+, which moved the C# baseline to .NET 8.

.NET 8 is an LTS release, first shipped November 2023, and its official
end-of-support date is **2026-11-10** — about three months from when this
ADR was written. .NET 9 (an STS release) reaches end-of-support the same
day, 2026-11-10 — Microsoft extended STS support windows to 24 months,
which lines .NET 9's clock up with .NET 8's
([Microsoft .NET blog](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)).
Retargeting to `net9.0` as a stopgap therefore buys no extra runway past
that date. .NET 10 (released November 2025) is the current LTS, supported
through November 2028, and is the only currently-supported destination.
Godot's own SDK, as of the current stable line (4.7.1), still officially
targets `net8.0`; `net9.0` works by manually setting `TargetFramework`, but
`net10.0` has open community reports of assembly-loading failures in Godot
and is not yet a safe default — verify against whatever `Godot.NET.Sdk`
version is current before assuming `net10.0` works.

## Decision

Stay on `net8.0` for now. Do not retarget to `net9.0`/`net10.0` as part of
routine work — that's a deliberate upgrade task with its own verification
pass (does the analyzer set still resolve correctly, does the Android export
path still work, does anything in `Directory.Build.props`-equivalent config
assume `net8.0`), not something to fold into an unrelated change.

## Consequences

### Positive

- Matches what Godot's SDK officially supports; no unsupported-configuration
  risk today.
- No churn to a project whose actual C# surface (25 files, no advanced
  language-version features) doesn't need anything `net8.0` lacks.

### Negative

- After 2026-11-10, this project is running on a runtime that Microsoft no
  longer patches. For a jam project with no deployed users, this is a
  low-severity risk, but it's still worth resolving in a real pass once
  .NET 8 support ends.

### Mitigations

- This ADR exists specifically so the clock is visible ahead of time rather
  than discovered as a surprise in November. When .NET 8's EOL date arrives
  or passes, treat retargeting as its own task: check Godot's current SDK
  version for its officially-supported `TargetFramework` before committing
  to it.
