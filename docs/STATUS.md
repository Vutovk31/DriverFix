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

These units are **not yet declared canonical-GitHub VERIFIED as a complete source tree**. The repository was newly created and source consolidation is the earliest physical gate.

## Earliest blocking gate

**P0 — consolidate the actual source required for DFX-001..DFX-014 into this repository and compile it.**

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

**Audio Diagnostics Pack** is now an explicit roadmap milestone.

Canonical acceptance case:

`Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

Success criterion: after evidence-backed repair and reboot/start under the same initial condition, the endpoint works without physical unplug/replug.

## Current priority

`consolidate source → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
