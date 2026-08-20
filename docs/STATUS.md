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
Current `main` contains:
- `IDriverBackupService` and `DriverBackupVerificationResult`;
- `PnpUtilDriverBackupService` accepting only an exact published OEM INF name matching `oem[0-9]+.inf`;
- exact command shape `pnputil.exe /export-driver <exact oem#.inf> <target>` with no wildcard export;
- empty-target precondition to avoid mixing prior artifacts with current backup evidence;
- non-zero PnPUtil exit treated as blocked;
- post-export disk verification requiring at least one `.inf`, no zero-length files and positive total bytes;
- explicit evidence text and exported-file list;
- no add/delete/install/uninstall/remove/restart driver mutation;
- `verification/verify_dfx011.py` binding exact-INF, empty-target, disk-evidence and no-mutation invariants.

Microsoft documents `/export-driver <oem#.inf | *> <target directory>`; DriverFix deliberately restricts this boundary to one exact `oem#.inf`. Real Windows PnPUtil export and C# compilation remain OPEN.

## Historical DFX lineage
Evidence-backed design exists through DFX-014.
DFX-001 through DFX-011 are physically present in canonical GitHub.

## Earliest blocking gate
**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-012 — controlled repair transaction that cannot execute unless DFX-011 backup result is verified.**

## Next engineering unit after consolidation
**DFX-015 — elevated worker executable + strict IPC contract.**

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

## Current priority
`consolidate DFX-001..014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
