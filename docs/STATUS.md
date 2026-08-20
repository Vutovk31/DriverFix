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
Current `main` contains:
- explicit transaction phases from pre-mutation through verification, reboot, rollback and terminal/manual-recovery states;
- `TransactionRecoveryPlanner` that resumes verification after known-applied repair mutation and resumes rollback verification after known-applied rollback mutation;
- ambiguous `MutationStarted` / `RollbackStarted` states routed to `ManualRecoveryRequired` rather than replaying mutation;
- durable `TransactionJournalEntry` evidence with transaction ID, target, phase, original driver identity, candidate/backup paths, reboot flag, timestamp and detail;
- `ITransactionJournal` plus `DriverFix.Persistence/JsonTransactionJournal`;
- temp-file write, `FileOptions.WriteThrough`, flush-to-disk and replace/move persistence semantics;
- terminal `Verified` / `RolledBack` journal entries excluded from incomplete recovery scans;
- corrupt JSON not auto-replayed;
- initial transaction-path containment checks;
- `IPrivilegeBoundary`, `PrivilegeCheckResult` and `WindowsPrivilegeBoundary` using the current Windows token/Administrators role;
- privilege check does not auto-elevate or launch arbitrary commands;
- `verification/verify_dfx014.py` binding 14 recovery/persistence/privilege invariants.

Real NTFS crash/reboot persistence, Windows token behavior and C# compilation remain OPEN. Journal cryptographic integrity/quarantine/ACL hardening remains later hardening rather than being falsely claimed in DFX-014.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is now complete**.

## Earliest blocking gate
**P0 — leave historical reconstruction mode and build the executable architecture.**

Nearest unfinished leaf: **DFX-015 — separate elevated worker executable + strict IPC/allow-list boundary for privileged driver mutation.**

## Next gates
`DFX-015 → canonical solution/build → real dotnet compile → win-x64 DriverFix.exe → Windows smoke → repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
