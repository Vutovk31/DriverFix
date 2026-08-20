# DriverFix — Canonical Engineering Status

Last canonicalization: 2026-08-20.

## Repository state

- Repository: `Vutovk31/DriverFix`
- Visibility: public
- Canonical branch: `main`
- License: MIT
- Development mode: hybrid automated + manual

## Evidence model

Two states must not be confused:

- **Historical/static verification** — evidence produced in previous development cycles.
- **Canonical physical presence** — implementation actually exists in the current GitHub source tree and participates in the current build.

Historical verification is useful evidence but does not substitute for source consolidation and compilation.

## Canonical physical source status

### DFX-001 — hardware inventory foundation: PRESENT / STATIC-CONTRACT VERIFIED

Current `main` now physically contains:

- `Directory.Build.props`;
- `DriverFix.Core/DriverFix.Core.csproj`;
- `DriverFix.Windows/DriverFix.Windows.csproj`;
- `DeviceInventoryItem`;
- `IDeviceInventoryProvider`;
- `IProcessRunner`, `ProcessRunner`, `ProcessResult`;
- `PnpUtilInventoryParser`;
- `PnpUtilDeviceInventoryProvider`;
- `verification/verify_dfx001.py`.

The provider is read-only and uses exactly:

`pnputil /enum-devices /connected /deviceids`

It includes Windows/platform guard, non-zero exit-code handling, bounded stderr evidence, cancellation propagation, Hardware/Compatible ID parsing, EN/RU field aliases, and no driver mutation operations.

The canonical files were fetched back from GitHub after write and inspected against the DFX-001 contract. A real C# compile remains OPEN until the repository has enough of the canonical source tree and a Windows/.NET build environment is used.

## Historical DFX lineage

The project has evidence-backed design/contract work through DFX-014:

- DFX-001..006 — device inventory and stable inventory boundary;
- DFX-007 — installed driver metadata;
- DFX-008 — evidence-backed diagnosis;
- DFX-009 — exact identifier compatibility matching;
- DFX-010 — read-only/trusted candidate discovery;
- DFX-011 — verified backup/export gate;
- DFX-012 — controlled repair transaction;
- DFX-013 — conservative rollback;
- DFX-014 — durable transaction/recovery and initial privilege boundary.

Only DFX-001 is currently declared canonical-GitHub physically present. Later units retain their historical verification evidence but still require physical consolidation into `main`.

## Earliest blocking gate

**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-002 — inventory parser fixture/integration evidence and inventory hardening continuation, without refactoring DFX-001.**

Do not skip directly to broad feature work based only on historical chat artifacts.

## Next engineering unit after consolidation

**DFX-015 — elevated worker executable + strict IPC contract.**

Target architecture:

- unelevated normal DriverFix process;
- separate `DriverFix.Elevated` worker requiring administrator privilege;
- one-shot authenticated/restricted IPC;
- strict operation allow-list, such as `InstallExactInf`, `RestartExactDevice`, `RestoreExactBackup`;
- exact INF and exact device identifiers;
- no arbitrary command line, `cmd.exe`, PowerShell payload, delete/remove/subdirs operation;
- structured result including reboot-required outcomes;
- UAC cancellation treated as an explicit non-success result.

## Product milestone added

**Audio Diagnostics Pack** is an explicit roadmap milestone.

Canonical acceptance case:

`Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

Success criterion: after evidence-backed repair and reboot/start under the same initial condition, the endpoint works without physical unplug/replug.

## Current priority

`consolidate DFX-001..014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
