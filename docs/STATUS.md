# DriverFix — Canonical Engineering Status

Last canonicalization: 2026-08-20.

## Repository state
- Repository: `Vutovk31/DriverFix`
- Visibility: public
- Canonical branch: `main`
- License: MIT
- Development mode: hybrid automated + manual

Historical/static verification never substitutes for physical source presence, compilation or real Windows execution.

## Canonical physical source status

### DFX-001..006 — inventory foundation: PRESENT / STATIC-REFERENCE VERIFIED
Read-only PnPUtil inventory, parser fixtures, CLI, typed failures, cancellation semantics and stable snapshot/result boundary are physically present. No mutation behavior.

### DFX-007 — installed driver metadata: PRESENT / STATIC-CONTRACT VERIFIED
Read-only `Win32_PnPSignedDriver` metadata provider, parser and normalized join to inventory snapshots are physically present. Real Windows/WMI execution remains OPEN.

### DFX-008 — evidence-backed diagnosis: PRESENT / STATIC-CONTRACT VERIFIED
Deterministic evidence-only diagnosis is physically present. Metadata join miss is not treated as proof of a missing driver. No version-age guessing or mutation.

### DFX-009 — exact identifier compatibility matching: PRESENT / STATIC-REFERENCE VERIFIED
Opaque exact matching is physically present across Hardware→Hardware, Hardware→Compatible, Compatible→Hardware and Compatible→Compatible tiers. Comparison is trim-only and case-insensitive, with no substring, VEN/DEV, manufacturer or class inference. DriverFix score is not Windows rank.

### DFX-010 — trusted read-only candidate discovery: PRESENT / STATIC-CONTRACT VERIFIED
Windows Update Agent driver candidate discovery, exact DFX-009 evidence gating, EULA/hidden blocking and no-download/no-install constraints are physically present. Real WUA COM execution remains OPEN.

### DFX-011 — verified exact-INF backup/export gate: PRESENT / STATIC-REFERENCE VERIFIED
Exact `oem#.inf` backup via `pnputil /export-driver` is physically present with empty-target, exit-code and on-disk artifact verification. Real Windows export remains OPEN.

### DFX-012 — controlled repair transaction: PRESENT / STATIC-CONTRACT VERIFIED
Repair requires verified compatibility, positive DriverFix match score, verified backup, target-bound before snapshot, one connected matching device, one exact existing INF, targeted restart and post-repair evidence. Reboot/unknown/unproven outcomes are not mislabeled VERIFIED. No destructive fallback.

### DFX-013 — conservative rollback: PRESENT / STATIC-CONTRACT VERIFIED
Rollback requires verified backup, target-bound original/failed snapshots, one connected matching device, one exact backup INF, targeted restart and proof that the original INF or version is restored on a healthy target. No destructive fallback.

### DFX-014 — durable transaction recovery + privilege boundary: PRESENT / STATIC-CONTRACT VERIFIED
Durable journal/recovery planning is physically present. Known-applied mutation resumes verification only; ambiguous mutation-started states require manual recovery rather than blind replay. Initial Windows privilege check is present without auto-elevation.

### DFX-015 — elevated worker + strict IPC boundary: PRESENT / STATIC-CONTRACT VERIFIED
Typed elevated operation allow-list, separate `DriverFix.Elevated` executable, `requireAdministrator`, one-shot named pipe, 256-bit nonce, UAC broker and worker-side validation are physically present. No free-form shell/PowerShell command boundary. Real UAC/IPC/PnPUtil execution remains OPEN.

### Canonical solution/build surface: PRESENT / STATIC-CONTRACT VERIFIED
`DriverFix.sln` contains all five canonical projects: `DriverFix.Core`, `DriverFix.Persistence`, `DriverFix.Windows`, `DriverFix.Cli`, and `DriverFix.Elevated`, with Debug/Release configurations.

### Windows compiler CI gate: RUNTIME VERIFIED GREEN
`.github/workflows/build.yml` runs on `windows-latest`, installs .NET 10, records SDK evidence, restores `DriverFix.sln`, and performs a Release build.

Real compiler evidence was obtained on 2026-08-20 through pull request #1. The first run restored all five projects successfully but failed with exactly one compiler error: `CS0201` in `DriverFix.Windows/Elevation/ElevatedWorkerBroker.cs` at the standalone `Process.Start(start) ?? throw ...` expression. The minimal fix replaced it with an explicit null check. A subsequent clean PR run completed `Restore canonical solution` and `Build canonical solution` successfully with the final diff containing only that source fix. The verified fix was squash-merged to `main` as commit `9cfdb6186272adc866027020a7290f96a38cfb12`.

Real compile status: **GREEN**. This does not yet imply real UAC/IPC/PnPUtil hardware behavior is GREEN.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is complete**. DFX-015 is physically present, and the canonical .NET 10 Windows Release compile gate is now runtime verified GREEN.

## Earliest blocking gate
**P0 — produce the first canonical `win-x64` executable build.**

Nearest unfinished leaf: **add a controlled `dotnet publish` gate for the user-facing executable, produce `DriverFix.exe`, verify the publish output and preserve the elevated worker boundary without falsely claiming hardware repair success.**

## Next gates
`win-x64 publish → DriverFix.exe → Windows executable smoke → UAC/IPC smoke → read-only hardware inventory → repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
