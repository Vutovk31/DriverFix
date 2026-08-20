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
Current `main` contains:
- `DriverUpdateCandidate` preserving update identity, provider/manufacturer/model/class/date, WUA `DriverHardwareID`, downloaded/hidden state and EULA state;
- `IDriverCandidateProvider`;
- `WindowsUpdateDriverCandidateProvider` using `Microsoft.Update.Session` / `CreateUpdateSearcher()` with `Type='Driver' and IsInstalled=0 and IsHidden=0`;
- `DriverCandidateJsonParser` supporting singleton and array JSON;
- conservative handling of WUA `DriverHardwareID` as ambiguous hardware-or-compatible evidence via `SourceMatchIdentifier`;
- `DriverCandidateEligibilityEvaluator` requiring an exact DFX-009 identifier match;
- EULA-not-accepted candidates blocked without implicit acceptance;
- hidden candidates blocked;
- no download, install, EULA acceptance, PnPUtil mutation or update-installer side effects;
- `verification/verify_dfx010.py` binding these invariants.

Microsoft documents `IWindowsDriverUpdate.DriverHardwareID` as a hardware ID or compatible ID the update must match to be installable, and WUA search supports `IsInstalled`/`IsHidden` criteria. Real WUA COM execution and C# compilation remain OPEN.

## Historical DFX lineage
Evidence-backed design exists through DFX-014.
DFX-001 through DFX-010 are physically present in canonical GitHub.

## Earliest blocking gate
**P0 — continue canonical source consolidation in order.**

Nearest unfinished leaf: **DFX-011 — verified exact-INF driver backup/export gate before any repair mutation.**

## Next engineering unit after consolidation
**DFX-015 — elevated worker executable + strict IPC contract.**

## Product milestone
**Audio Diagnostics Pack** remains explicit. Canonical acceptance case: `Windows 11 + headphones already connected → no usable sound/endpoint after startup → unplug/replug makes it work`.

## Current priority
`consolidate DFX-001..014 → DFX-015 → real compile → win-x64 executable → Windows smoke → hardware repair/rollback → Audio Diagnostics`
