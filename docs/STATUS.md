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
Current `main` contains:
- `RepairRequest`, `RepairResult` and typed `RepairOutcome`;
- mandatory verified compatibility and positive DriverFix match score;
- mandatory DFX-011 verified backup with positive artifact bytes;
- before-snapshot bound to the exact target InstanceId;
- blast-radius gate `ConnectedMatchingDeviceCount == 1` because `pnputil /add-driver <inf> /install` may update any matching devices;
- exact existing `.inf` path with wildcard rejection;
- exact install shape `pnputil /add-driver <exact.inf> /install`;
- targeted restart `pnputil /restart-device <InstanceId>`;
- reboot-required handling for 3010/1641 without declaring VERIFIED before post-reboot evidence;
- install rejection handling including exit 259;
- unknown post-mutation process state escalated to `ManualRecoveryRequired` rather than blind retry;
- post-repair snapshot verification requiring same target, healthy state and either driver identity change or clearing of the original non-zero PnP problem;
- unproven repair routed to `RollbackRequired`;
- no destructive fallback (`/delete-driver`, `/remove-device`, `/uninstall`, `/force`, `/subdirs`);
- `verification/verify_dfx012.py` binding these invariants.

Microsoft documents that `/install` installs/updates on any matching devices and that `/restart-device <instance ID>` targets a specific device. Real Windows repair execution and C# compilation remain OPEN.

## Historical DFX lineage
Evidence-backed design exists through DFX-014.
DFX-001 through DFX-012 are physically present in canonical GitHub.

## Earliest blocking gate
**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-013 — conservative rollback using the verified backup, with the same matching-device blast-radius protection before restore mutation.**

## Next engineering unit after consolidation
**DFX-015 — elevated worker executable + strict IPC contract.**

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

## Current priority
`consolidate DFX-001..014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
