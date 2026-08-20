# DriverFix — Canonical Roadmap

Status vocabulary: `OPEN`, `IN PROGRESS`, `STATIC VERIFIED`, `WINDOWS VERIFIED`, `DONE`, `BLOCKED`.

## Phase 0 — Canonical repository and hybrid control

Goal: one physical source tree, one ordered development history, one canonical reporting thread.

- [x] Public GitHub repository created.
- [x] `main` designated canonical physical source of truth.
- [x] Product concept documented.
- [x] Hybrid automation/manual workflow documented.
- [x] Consolidate the historical DFX-001..DFX-014 implementation into the repository and prove the tree is internally consistent.
- [x] Add canonical solution/projects for the consolidated source.
- [ ] Obtain real compiler evidence for the canonical solution.

**Gate:** repository contains the actual source required to build DriverFix; historical chat evidence alone is insufficient.

## Phase 1 — Safe repair core

Historical design/contract work exists for these units; each becomes canonical only when present and verified in the repository.

- DFX-001 — Device inventory.
- DFX-002 — PnPUtil parser fixtures/hardening.
- DFX-003 — inventory CLI.
- DFX-004 — inventory integration evidence.
- DFX-005 — typed inventory failures.
- DFX-006 — inventory completion boundary.
- DFX-007 — current installed-driver metadata.
- DFX-008 — evidence-backed diagnosis classification.
- DFX-009 — exact Hardware/Compatible ID matching.
- DFX-010 — trusted/read-only candidate discovery.
- DFX-011 — verified driver backup/export.
- DFX-012 — controlled repair transaction.
- DFX-013 — conservative rollback.
- DFX-014 — transaction journal and crash/reboot recovery model.
- **DFX-015 — elevated worker + strict IPC command contract.**
- DFX-016 — repair transaction orchestrator.
- DFX-017 — reboot-safe rollback.
- DFX-018 — startup recovery bootstrap.
- DFX-019..023 — journal trust/storage/composition hardening.

### Immediate order

1. Observe real .NET 10 compiler evidence from the canonical Windows CI gate.
2. If RED, fix the earliest compiler failure only and rebuild.
3. If GREEN, publish `win-x64` and produce the first real executable.
4. Continue DFX-016..023 only where real build/runtime evidence shows they are required before hardware smoke.

## Phase 2 — Compile and executable

This phase has priority over broad feature expansion.

- [x] Canonical `.sln` / project references.
- [ ] Restore/build under a supported .NET SDK — CI gate physically present, completed result pending.
- [ ] Resolve compile errors from the consolidated tree.
- [ ] Build unelevated main application.
- [ ] Build separate elevated worker.
- [ ] Publish `win-x64` package.
- [ ] Produce real `DriverFix.exe`.

**Gate:** actual Windows executable launches from the canonical repository build.

## Phase 3 — Windows hardware smoke

Use non-critical test devices first.

- [ ] Inventory real devices.
- [ ] Join live installed-driver metadata.
- [ ] Compare diagnosis with Device Manager evidence.
- [ ] Discover a real candidate without installing it.
- [ ] Export and verify current driver package.
- [ ] Execute one controlled repair on a disposable/non-critical device.
- [ ] Verify actual post-state.
- [ ] Execute rollback and prove original state where possible.
- [ ] Kill/reboot during defined transaction phases and verify no ambiguous mutation replay.

**Gate:** `detect → diagnose → backup → repair → verify → rollback` works on real Windows hardware.

## Phase 4 — Audio Diagnostics Pack

### A1 — Inventory and endpoint evidence

- [ ] Enumerate relevant audio PnP devices and installed packages.
- [ ] Enumerate playback/capture endpoints and endpoint state.
- [ ] Record Windows build and audio-service state.
- [ ] Correlate controller/codec/USB/Bluetooth/HDMI devices to endpoints where evidence permits.

### A2 — Stack/component diagnostics

- [ ] Check `audiosrv` and endpoint-building service state.
- [ ] Record OEM extension INF/component/APO evidence where safely observable.
- [ ] Detect endpoint redetection/restart transitions.
- [ ] Capture power/restart-related evidence without speculative diagnosis.

### A3 — Windows 11 unplug/replug acceptance case

Canonical symptom:

`headphones already connected → Windows starts → audio unavailable → unplug/replug → audio works`.

Required test:

1. capture baseline before unplug/replug;
2. capture endpoint/device/service state;
3. perform or instruct a controlled redetection/restart step;
4. isolate the smallest evidence-backed repair layer;
5. back up before driver mutation;
6. repair;
7. reboot/start under the same initial condition;
8. verify endpoint is available **without physical unplug/replug**;
9. rollback if the behavioral test fails.

**Gate:** original behavioral symptom is proven fixed, not just driver version changed.

### A4 — Microphone/capture path

Apply the same approach to microphone/capture failures, including headset and Bluetooth capture endpoints.

## Phase 5 — Broader class diagnostics

After core and audio are proven:

- chipset/motherboard/system devices;
- USB;
- Wi-Fi/Ethernet;
- Bluetooth;
- storage;
- GPU/display-related driver chains;
- webcams and common peripherals.

Each domain must define at least one behavioral acceptance case before repair automation expands.

## Phase 6 — Product UX and distribution

- clear symptom/evidence/explanation UI;
- repair preview and explicit risk boundary;
- backup/rollback history;
- portable/installer packaging as justified;
- signed release path when justified by actual distribution needs;
- documentation and contribution workflow.

## Explicit non-goals for the MVP

- “Update everything” button.
- Driver-version chasing without a symptom/evidence case.
- Arbitrary third-party driver download sites.
- Automatic BIOS/UEFI flashing.
- Destructive Driver Store cleanup as a routine fallback.
- Premature cloud infrastructure, accounts, analytics, paid services, SEO or multi-agent orchestration.
