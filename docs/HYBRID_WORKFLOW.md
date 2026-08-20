# DriverFix — Hybrid Development Workflow

## Canonical sources

1. **GitHub `Vutovk31/DriverFix`, branch `main`** — canonical physical source of truth.
2. **The canonical DriverFix ChatGPT thread** — canonical control/reporting thread for automated and manual development decisions.

There must not be separate competing development histories in different chats.

## Hybrid rule

The user may commit or edit the repository at any time. Automated development must treat those changes as new input, not as noise to overwrite.

Before every automated cycle:

1. read the current `main` head;
2. inspect commits/files changed since the previous observed head;
3. classify manual/user changes and automation changes;
4. preserve compatible manual changes;
5. if a manual change conflicts with a verified invariant, stop the conflicting mutation, produce evidence/RCA, and resolve explicitly rather than silently reverting it;
6. select the highest-value unfinished leaf unit from the current repository state.

## Concurrency rules

- Never force-push or rewrite history.
- Never overwrite a changed file based on an old snapshot.
- Re-read a file/ref immediately before updating it.
- Direct commits to `main` are acceptable for small, non-overlapping, fully verified changes.
- If overlap/concurrency risk is material, use a short-lived branch/PR and reconcile against current `main` before merge.
- If the user changes the same unit during an automation cycle, current repository state wins; the cycle must re-evaluate instead of replaying its stale patch.

## AIS/AES cycle

Each cycle must:

1. **RECOVER STATE** — inspect current repository and recent commits.
2. **FREEZE VERIFIED WORK** — do not replay completed work without new evidence.
3. **SELECT ONE LEAF** — choose the smallest highest-value unfinished unit.
4. **DEFINE CONTRACT** — outcome, evidence, non-goals, stop condition.
5. **EXECUTE REAL DELTA** — code/test/build/docs only when it advances the product. Status-only cycles are invalid.
6. **VERIFY STRONGLY** — prefer compile/runtime/hardware evidence over repeated static checks when available.
7. **CLASSIFY FAILURE** — evidence → RCA → minimal fix; no speculative patching.
8. **COMMIT** — update canonical GitHub state without destroying concurrent work.
9. **REPORT IN CANONICAL THREAD** — include observed manual changes, exact delta, verification, commit/PR, new VERIFIED state, remaining blocker and exact next step.

## Information-gain rule

Do not keep adding architecture merely because it can be added. When a real compile, Windows runtime check, hardware smoke test or behavioral test can reveal more information, prefer that gate.

## Current execution priority

`repository consolidation → DFX-015 → compile/build → DriverFix.exe → Windows hardware smoke → repair/rollback field test → Audio Diagnostics Pack`

Audio work may add small enabling interfaces earlier only when required by the build; it must not postpone the first real executable.

## Safety and public-repository rule

The repository is public. Never commit:

- passwords, access tokens, private keys or cookies;
- personal/private machine data not intentionally sanitized;
- proprietary driver packages without redistribution rights;
- copyrighted third-party code without compatible licensing/attribution.

## Completion standard

A unit is not DONE because text was written about it. DONE means the strongest currently available evidence supports the outcome. Deferred Windows/runtime evidence must remain explicitly OPEN until executed.
