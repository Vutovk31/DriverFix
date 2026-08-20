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

Current `main` physically contains the read-only PnPUtil inventory foundation, including `DeviceInventoryItem`, provider/process abstractions, `PnpUtilInventoryParser`, `PnpUtilDeviceInventoryProvider`, project files, and `verification/verify_dfx001.py`.

The provider uses exactly:

`pnputil /enum-devices /connected /deviceids`

It includes Windows/platform guard, non-zero exit-code handling, bounded stderr evidence, cancellation propagation, Hardware/Compatible ID parsing, EN/RU field aliases, and no driver mutation operations.

### DFX-002 — parser fixture/hardening evidence: PRESENT / FIXTURE-CONTRACT VERIFIED

Current `main` also contains deterministic parser fixtures and `verification/verify_dfx002.py` covering:

- decorated problem codes such as `52 (0x34) [...]`;
- Hardware ID and Compatible ID continuation lines;
- synthetic Russian field aliases;
- multiple devices without relying on a blank-line separator;
- Code 28 extraction on the second device;
- empty-output and no-mutation parser invariants.

The DFX-002 delta does not refactor the frozen DFX-001 production parser. The strongest available evidence is fixture/reference verification; real C# compilation remains OPEN.

### DFX-003 — inventory CLI/presentation boundary: PRESENT / STATIC-CONTRACT VERIFIED

Current `main` contains `DriverFix.Cli` with:

- `DriverFix.Cli.csproj` targeting `net10.0-windows`;
- references to canonical `DriverFix.Core` and `DriverFix.Windows` projects;
- `Program.cs` composing the existing `PnpUtilDeviceInventoryProvider` and `ProcessRunner`;
- `DeviceInventoryTextFormatter` rendering connected-device count, description, Instance ID, class, manufacturer, status, Problem Code, Hardware IDs and Compatible IDs;
- stable provider-order presentation;
- explicit exit codes for success, general failure and unsupported platform;
- `verification/verify_dfx003.py` enforcing the presentation contract and no-mutation invariant.

DFX-003 does not change the frozen DFX-001/002 parser/provider behavior. Real `dotnet build` and Windows CLI execution remain OPEN.

### DFX-004 — inventory integration evidence: PRESENT / FIXTURE-CONTRACT VERIFIED

Current `main` contains deterministic end-to-end fixtures and `verification/verify_dfx004.py` covering:

- English PnPUtil text → parser semantics → device models → exact CLI text;
- synthetic Russian PnPUtil text → parser semantics → device models → exact CLI text;
- missing manufacturer fallback to `unknown`;
- decorated Code 28 extraction in the integrated output;
- Hardware/Compatible ID continuation preservation;
- empty inventory output;
- binding checks against the frozen parser/formatter contracts when run from a repository checkout;
- no driver mutation operations.

The DFX-004 delta adds verification evidence only and does not refactor DFX-001/002/003 production code. Real C# compilation and real Windows PnPUtil execution remain OPEN.

### DFX-005 — typed inventory failure boundary: PRESENT / STATIC-REFERENCE VERIFIED

Current `main` now contains a typed inventory failure taxonomy:

- `PlatformUnsupported`;
- `ProcessLaunchFailed`;
- `ToolReturnedNonZero`;
- `UnexpectedFailure`.

`InventoryProviderException` preserves the failure kind, optional process exit code, bounded stderr evidence and inner exception. `PnpUtilDeviceInventoryProvider` now converts platform/process/non-zero/parser failures into these typed outcomes while preserving `OperationCanceledException` as cancellation instead of misclassifying it as a provider failure. The read-only PnPUtil command remains unchanged.

`DriverFix.Cli` surfaces the typed kind, retains exit code `2` for unsupported platform, uses `1` for provider/general failure, and uses `3` for cancellation. `verification/verify_dfx005.py` binds the taxonomy, provider behavior, cancellation propagation, bounded stderr and no-mutation invariants.

Real C# compilation and Windows process-failure/cancellation execution remain OPEN.

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

DFX-001 through DFX-005 are currently declared canonical-GitHub physically present. Later units retain historical verification evidence but still require physical consolidation into `main`.

## Earliest blocking gate

**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-006 — stable inventory snapshot/result boundary so callers receive either a captured device set or typed failure evidence without losing cancellation semantics.**

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
