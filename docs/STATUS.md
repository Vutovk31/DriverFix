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
`.github/workflows/build.yml` runs on `windows-latest`, installs .NET 10, records SDK evidence, restores `DriverFix.sln`, and performs a Release build. Pull request #1 established the first real compiler GREEN after one minimal CS0201 fix in `ElevatedWorkerBroker.cs`; the verified fix was squash-merged as `9cfdb6186272adc866027020a7290f96a38cfb12`.

### Canonical win-x64 publish gate: RUNTIME VERIFIED GREEN
Pull request #2 changed the user-facing assembly name to `DriverFix` and extended the Windows build workflow to publish both `DriverFix.exe` and `DriverFix.Elevated.exe` as self-contained single-file `win-x64` executables. The clean PR run completed restore, Release build, both publish steps, executable existence/non-zero-size verification, SHA-256 logging and artifact upload successfully. The verified publish changes were squash-merged to `main` as `e03cdd929d77ed75281e9915fed7328fca0f804f`.

Clean publish evidence from run `32376316918`:
- `DriverFix.exe`: 73,640,230 bytes; SHA-256 `E366454268116D9EDEC5CF9D7A7EC00E65FD5FC2FF982DE0E07833E8DF348B72`.
- `DriverFix.Elevated.exe`: 73,640,284 bytes; SHA-256 `CA9888F0E4516BD22330C3F9BF9631DA940857C5E6CE8FAEC9632EF5752849F9`.
- artifact: `DriverFix-win-x64`, 65,847,693 bytes compressed, artifact SHA-256 `c65870749c619777f96ab5605c6d7380d2649e45535168e3806d3d9527e808ca`, retained for 7 days.

This proves build/publish/package integrity only. It does not yet prove real Windows launch, UAC/IPC, hardware inventory, repair or rollback behavior.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is complete**. DFX-015 is physically present. Real Windows Release compile and canonical win-x64 publish are runtime verified GREEN.

## Earliest blocking gate
**P0 — perform the first real Windows executable smoke test.**

Nearest unfinished leaf: **download the canonical `DriverFix-win-x64` package on a real Windows machine, launch `DriverFix.exe`, confirm process startup/read-only inventory behavior, and keep privileged mutation/UAC tests separate until the basic executable smoke is proven.**

## Next gates
`Windows executable smoke → UAC/IPC smoke → read-only hardware inventory → repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
