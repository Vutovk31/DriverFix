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
Current `main` contains:
- typed `ElevatedOperation` allow-list with only `InstallExactInf`, `RestartExactDevice`, and `RestoreExactBackup`;
- typed `ElevatedRequest` / `ElevatedResponse` with a one-time nonce;
- `ElevatedOperationValidator` requiring exact `.inf` paths, rejecting wildcards and requiring exact InstanceId for targeted restart;
- separate `DriverFix.Elevated` executable project targeting Windows;
- worker manifest with `requireAdministrator`;
- unelevated `ElevatedWorkerBroker` creating a one-connection named pipe with `PipeOptions.CurrentUserOnly`;
- 256-bit cryptographic nonce and random pipe name;
- UAC launch through `UseShellExecute=true` + `Verb="runas"`;
- worker-side nonce comparison before command dispatch;
- worker constructs PnPUtil argument arrays internally from the typed operation; it does not execute free-form command text;
- no `cmd.exe`, PowerShell or arbitrary script payload boundary;
- reboot-required exit codes 3010/1641 preserved in the structured response;
- `verification/verify_dfx015.py` binding 16 IPC/UAC/allow-list invariants.

Real UAC consent, named-pipe exchange, PnPUtil mutation and C# compilation remain OPEN. This status is intentionally not Windows runtime GREEN.

## Historical DFX lineage
Canonical physical consolidation of **DFX-001 through DFX-014 is complete**, and the first post-consolidation privileged execution boundary DFX-015 is now physically present.

## Earliest blocking gate
**P0 — obtain real compiler evidence from the complete canonical source tree.**

Nearest unfinished leaf: **create/update the canonical solution/build surface to include Core, Windows, Persistence, CLI and Elevated projects, then run real `dotnet build` and fix the earliest compile failure.**

## Next gates
`canonical solution/build → real dotnet compile → win-x64 DriverFix.exe → Windows smoke → repair/rollback field test → Audio Diagnostics`

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.
