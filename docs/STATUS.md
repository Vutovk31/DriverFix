# DriverFix — Canonical Engineering Status

Last canonicalization: 2026-08-20.

## Repository state
- Repository: `Vutovk31/DriverFix`
- Visibility: public
- Canonical branch: `main`
- License: MIT
- Development mode: hybrid automated + manual

## Evidence model
- **Historical/static verification** — prior evidence.
- **Canonical physical presence** — implementation exists in current GitHub source tree.

Historical verification does not substitute for compilation or real Windows execution.

## Canonical physical source status

### DFX-001..006 — inventory foundation: PRESENT / STATIC-REFERENCE VERIFIED
Read-only PnPUtil inventory, parser fixtures, CLI presentation, typed failures, cancellation semantics and stable snapshot/result boundary are present. No mutation behavior.

### DFX-007 — installed driver metadata: PRESENT / STATIC-CONTRACT VERIFIED
Read-only `Win32_PnPSignedDriver` metadata provider, parser and normalized join to inventory snapshots are present. Real Windows/WMI execution remains OPEN.

### DFX-008 — evidence-backed diagnosis: PRESENT / STATIC-CONTRACT VERIFIED
Deterministic diagnosis precedence is present: Code 28 → `DriverMissing/High`; other positive PnP codes → `DeviceProblem/High`; metadata join miss without PnP error → `DriverMetadataMissing/Medium`; unsigned → `DriverUnsigned/High`; missing version → `DriverVersionUnknown/Medium`; otherwise `Healthy/High`. No speculative version-age inference or mutation.

### DFX-009 — exact identifier compatibility matching: PRESENT / STATIC-REFERENCE VERIFIED
Current `main` now contains:
- `DriverCandidateIdentifiers` with candidate Hardware IDs and Compatible IDs;
- `DriverIdentifierMatchKind` for the four exact match channels;
- `DriverIdentifierMatch` with explicit kind, internal DriverFix score and matched identifiers;
- `DriverIdentifierMatcher` treating identifiers as opaque strings, trim-only and case-insensitive;
- tier priority: device Hardware→candidate Hardware `4000`, Hardware→Compatible `3000`, Compatible→Hardware `2000`, Compatible→Compatible `1000`, with small position penalty inside each tier;
- no substring matching, VEN/DEV parsing, manufacturer inference or class inference;
- score is an internal DriverFix compatibility tier and is **not** Windows driver rank;
- no install/delete/remove/restart behavior;
- `verification/verify_dfx009.py` binding exact-match/no-guessing invariants.

Real C# compilation and real candidate matching on Windows remain OPEN.

## Historical DFX lineage
Evidence-backed design exists through DFX-014.
DFX-001 through DFX-009 are now physically present in canonical GitHub. Later units retain historical evidence but still require physical consolidation into `main`.

## Earliest blocking gate
**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-010 — read-only/trusted candidate discovery from Windows Update Agent, preserving identifier evidence and blocking mutation/EULA side effects.**

## Next engineering unit after consolidation
**DFX-015 — elevated worker executable + strict IPC contract.**

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

## Current priority
`consolidate DFX-001..014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
