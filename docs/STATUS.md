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
Current `main` contains `DriverFix.sln` with all five canonical projects:
- `DriverFix.Core`;
- `DriverFix.Persistence`;
- `DriverFix.Windows`;
- `DriverFix.Cli`;
- `DriverFix.Elevated`.

Debug/Release configurations are present. Windows-facing projects target `net10.0-windows`; the elevated project keeps its UAC manifest. `verification/verify_build_surface.py` verifies the project graph and escalates to real `dotnet build DriverFix.sln -c Release --nologo` when a .NET SDK is physically available.

### Windows compiler CI gate: PRESENT / RUNTIME RESULT PENDING
`.github/workflows/build.yml` now provides a canonical real-build path on `windows-latest`: checkout canonical source, install .NET 10 SDK, capture `dotnet --info`, restore `DriverFix.sln`, then run Release `dotnet build` with `--no-restore`.

The workflow itself is physically present and its action versions/SDK channel are evidence-backed. A completed GitHub Actions job/log has not yet been observed through the available connector, so **real compiler GREEN remains OPEN**. The assistant shell also cannot resolve external hosts, so that environment failure is not treated as a source-code failure.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is complete**. DFX-015, the canonical solution/build surface and the real Windows compiler gate are physically present.

## Earliest blocking gate
**P0 — observe the first completed compiler result from the canonical Windows CI gate.**

Nearest unfinished leaf: **read the `DriverFix Build` job result; if it fails, capture the earliest compiler error and apply only the minimum fix; if it passes, mark real compile GREEN and move to `win-x64` publish.**

## Next gates
`observe CI compile result → fix earliest compiler failure or mark compile GREEN → win-x64 publish → DriverFix.exe → Windows smoke → repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
