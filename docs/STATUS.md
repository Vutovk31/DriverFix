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
Current `main` contains:
- `RollbackRequest`, `RollbackResult` and typed `RollbackOutcome`;
- mandatory verified backup with positive bytes;
- original and failed snapshots bound to the exact target InstanceId;
- rollback blast-radius gate `ConnectedMatchingDeviceCount == 1` because restore uses `pnputil /add-driver <backup.inf> /install`, which Windows documents as applying to matching devices;
- one exact existing backup `.inf` with wildcard rejection;
- restore shape `pnputil /add-driver <backup.inf> /install`;
- targeted restart `pnputil /restart-device <InstanceId>`;
- reboot-required handling for 3010/1641 without replaying restore or declaring `RolledBack`;
- exit 259 and unknown mutation state escalated to `ManualRecoveryRequired` with no force/delete fallback;
- post-rollback verification requiring the same target, healthy PnP state, and restoration of the original INF or original driver version;
- no `/delete-driver`, `/remove-device`, `/uninstall`, `/force` or `/subdirs` fallback;
- `verification/verify_dfx013.py` binding these invariants.

Microsoft documents `/install` as installing/updating on any matching devices; DFX-013 therefore deliberately mirrors DFX-012's single-matching-device gate. Real Windows rollback execution and C# compilation remain OPEN.

## Historical DFX lineage
Evidence-backed design exists through DFX-014.
DFX-001 through DFX-013 are physically present in canonical GitHub.

## Earliest blocking gate
**P0 — finish canonical source consolidation.**

Nearest unfinished leaf: **DFX-014 — durable transaction journal/recovery state plus initial privilege boundary, reconciled with the now-physical DFX-012 repair and DFX-013 rollback contracts.**

## Next engineering unit after consolidation
**DFX-015 — elevated worker executable + strict IPC contract.**

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

## Current priority
`DFX-014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
