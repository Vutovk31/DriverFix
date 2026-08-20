# DriverFix — Canonical Engineering Status

Last canonicalization: 2026-08-21.

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

### DFX-007 — installed driver metadata: PRESENT / WINDOWS CI RUNTIME VERIFIED
Read-only `Win32_PnPSignedDriver` metadata provider, parser and normalized join to inventory snapshots are physically present. Pull request #5 added packaged `DriverFix.exe --driver-metadata-smoke` and Windows CI run `32386609356` executed the existing CIM/PowerShell provider successfully on Windows Server 2025, returning **63 installed driver metadata records** with process exit code `0`. This proves the packaged DFX-007 provider/parser runtime path on a real Windows runner; target-workstation metadata behavior remains OPEN.

### DFX-008 — evidence-backed diagnosis: PRESENT / WINDOWS CI RUNTIME VERIFIED
Deterministic evidence-only diagnosis is physically present. Metadata join miss is not treated as proof of a missing driver, and no version-age guessing or mutation is performed. Pull request #10 added packaged `DriverFix.exe --diagnosis-smoke`, which executes live PnP inventory → installed-driver metadata join → `DiagnosisEngine` on Windows. Windows CI run `32419313150` completed the full pipeline successfully and produced **62 diagnoses for 62 connected devices**: **1 `DriverMissing`**, **2 `DriverVersionUnknown`**, and **59 `Healthy`**. The same inventory contained one PnP device with **Problem Code 28**, exercising the explicit evidence-backed `DriverMissing` branch at runtime. Target-workstation diagnosis behavior remains OPEN.

### DFX-009 — exact identifier compatibility matching: PRESENT / STATIC-REFERENCE VERIFIED
Opaque exact matching is physically present across Hardware→Hardware, Hardware→Compatible, Compatible→Hardware and Compatible→Compatible tiers. Comparison is trim-only and case-insensitive, with no substring, VEN/DEV, manufacturer or class inference. DriverFix score is not Windows rank.

### DFX-010 — trusted read-only candidate discovery: PRESENT / WINDOWS CI RUNTIME VERIFIED
Windows Update Agent driver candidate discovery, exact DFX-009 evidence gating, EULA/hidden blocking and no-download/no-install constraints are physically present. Pull request #6 added packaged `DriverFix.exe --wua-candidate-smoke` with a 90-second cancellation boundary and Windows CI run `32391879604` executed the existing WUA search path successfully on Windows Server 2025 with process exit code `0`, returning **0 available driver candidates**. Zero candidates is a valid search result; the runtime evidence proves the packaged WUA COM discovery path executes without invoking download/install. Target-workstation WUA behavior remains OPEN.

### DFX-011 — verified exact-INF backup/export gate: PRESENT / WINDOWS CI RUNTIME VERIFIED
Exact `oem#.inf` backup via `pnputil /export-driver` is physically present with empty-target, exit-code and on-disk artifact verification. Pull request #8 added packaged `DriverFix.exe --backup-export-smoke`, which selects an exact installed `oem#.inf`, exports it through the existing `PnpUtilDriverBackupService` into a unique empty temporary directory, and requires positive on-disk file/byte evidence before deleting the temporary copy. Windows CI run `32408648589` completed Release build, both publishes, package verification, all prior smoke gates, exact-INF export and artifact upload successfully. Runtime evidence: **`oem5.inf`**, **4 exported files**, **1 INF**, **4,544,057 bytes** verified on disk. No install/delete/restart/repair/rollback operation was invoked. Target-workstation backup/export behavior remains OPEN.

### DFX-012 — controlled repair transaction: PRESENT / WINDOWS CI PREFLIGHT RUNTIME VERIFIED
Repair requires verified compatibility, positive DriverFix match score, verified backup, target-bound before snapshot, one connected matching device, one exact existing INF, targeted restart and post-repair evidence. Pull request #9 extracted these mutation-admission checks into `RepairPreflightService`; `PnpUtilRepairExecutor` now requires that shared gate before the first `ProcessRunner`/PnPUtil call. Packaged `DriverFix.exe --repair-preflight-smoke` supplies an intentionally unverified backup and Windows CI run `32413920550` proved the request is rejected with `Repair preflight negative smoke: BLOCKED` and exact evidence `Verified driver backup is required before repair.` The smoke path does not construct or invoke `ProcessRunner`, PnPUtil, install, restart, repair or rollback. This proves the negative preflight/no-mutation boundary at runtime; an actual controlled repair mutation and post-state verification remain OPEN.

### DFX-013 — conservative rollback: PRESENT / STATIC-CONTRACT VERIFIED
Rollback requires verified backup, target-bound original/failed snapshots, one connected matching device, one exact backup INF, targeted restart and proof that the original INF or version is restored on a healthy target. No destructive fallback.

### DFX-014 — durable transaction recovery + privilege boundary: PRESENT / STATIC-CONTRACT VERIFIED
Durable journal/recovery planning is physically present. Known-applied mutation resumes verification only; ambiguous mutation-started states require manual recovery rather than blind replay. Initial Windows privilege check is present without auto-elevation.

### DFX-015 — elevated worker + strict IPC boundary: PRESENT / WINDOWS BUILD VERIFIED
Typed elevated operation allow-list, separate `DriverFix.Elevated` executable, `requireAdministrator`, one-shot named pipe, 256-bit nonce, UAC broker and worker-side validation are physically present. A dedicated `Probe` operation and `DriverFix.exe --elevation-smoke` entry point now exercise broker → UAC launch request → named pipe → nonce validation → worker response without accepting mutation arguments or invoking PnPUtil. Windows CI run `32380218686` verified Release build, both win-x64 publishes, package verification and the existing read-only inventory smoke with this path present. Real interactive UAC acceptance and IPC completion on a non-CI workstation remain OPEN.

### Canonical solution/build surface: PRESENT / STATIC-CONTRACT VERIFIED
`DriverFix.sln` contains all five canonical projects: `DriverFix.Core`, `DriverFix.Persistence`, `DriverFix.Windows`, `DriverFix.Cli`, and `DriverFix.Elevated`, with Debug/Release configurations.

### Windows compiler CI gate: RUNTIME VERIFIED GREEN
`.github/workflows/build.yml` runs on `windows-latest`, installs .NET 10, records SDK evidence, restores `DriverFix.sln`, and performs a Release build. Pull request #1 established the first real compiler GREEN after one minimal CS0201 fix in `ElevatedWorkerBroker.cs`; the verified fix was squash-merged as `9cfdb6186272adc866027020a7290f96a38cfb12`.

### Canonical win-x64 publish gate: RUNTIME VERIFIED GREEN
Pull request #2 changed the user-facing assembly name to `DriverFix` and extended the Windows build workflow to publish both `DriverFix.exe` and `DriverFix.Elevated.exe` as self-contained single-file `win-x64` executables. The clean PR run completed restore, Release build, both publish steps, executable existence/non-zero-size verification, SHA-256 logging and artifact upload successfully. The verified publish changes were squash-merged to `main` as `e03cdd929d77ed75281e9915fed7328fca0f804f`.

### Windows executable smoke: RUNTIME VERIFIED GREEN
Pull request #3 added a post-publish read-only smoke that launches the exact packaged `dist/DriverFix.exe`, captures stdout/stderr, requires process exit code `0`, and requires `Connected devices: <number>` evidence before artifact upload. Clean Windows run `32377385614` passed restore, Release build, both publish steps, package verification, the executable smoke, and artifact upload. `DriverFix.exe` enumerated **67 connected devices** on Windows Server 2025, including real PnP instance IDs, hardware/compatible IDs, statuses and one device reporting `Problem Code: 28`. The smoke gate was squash-merged as `9724e030481ca74772ea72afcfc35614cdd438bd`.

### Combined workstation read-only smoke: RUNTIME VERIFIED GREEN ON WINDOWS CI
Pull request #7 added packaged `DriverFix.exe --workstation-readonly-smoke`, which runs the existing PnP inventory, installed-driver metadata and WUA candidate-discovery paths sequentially without UAC or mutation. Windows CI run `32402977723` completed Release build, both publishes, package verification, all existing smoke gates, the new combined gate and artifact upload successfully. Combined evidence was `Workstation read-only smoke: PASS`, **62 connected devices**, **63 installed driver metadata records**, and **0 Windows Update driver candidates**. This command is intended to reduce the target-workstation read-only field check to one bounded command; execution on the user's physical workstation remains OPEN.

The packaged user-facing executable therefore has runtime evidence for read-only inventory, installed-driver metadata, evidence-backed diagnosis, WUA discovery, verified exact-INF backup/export, and the DFX-012 negative repair-preflight/no-mutation boundary on real Windows CI runners. This still does not prove interactive UAC/IPC completion, target-workstation behavior, an actual controlled repair, post-repair verification or rollback.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is complete**. DFX-015 is physically present and Windows-build verified. Real Windows Release compile, canonical win-x64 publish, packaged read-only inventory smoke, DFX-007 installed-driver metadata, DFX-008 evidence-backed diagnosis, DFX-010 read-only WUA candidate discovery, DFX-011 exact-INF backup/export, DFX-012 negative repair preflight, and the combined workstation read-only command are runtime verified on Windows CI.

## Earliest blocking gate
**P0 — exercise the packaged read-only evidence bundle and the no-mutation elevated probe on a non-CI Windows workstation.**

Nearest unfinished leaf: **run `DriverFix.exe --workstation-readonly-smoke` on the target Windows workstation and require `Workstation read-only smoke: PASS`; then run `DriverFix.exe --elevation-smoke`, approve UAC, and require the exact evidence `Elevated IPC probe completed; no PnPUtil command was executed.`**

## Next gates
`target Windows workstation combined read-only smoke → UAC/IPC no-mutation smoke → target-workstation backup field test → controlled repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
