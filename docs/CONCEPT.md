# DriverFix — Canonical Product Concept

## Mission

DriverFix finds concrete Windows/hardware/driver incompatibilities, explains the evidence, chooses the smallest compatible repair, creates a recovery point, applies the change, verifies the original symptom, and rolls back when verification fails.

It is not a generic “update every driver” product.

## Canonical execution loop

`detect → diagnose → match → backup → repair → verify → rollback`

Every repair must be traceable to evidence collected before the mutation and to observable post-repair evidence afterwards.

## Diagnostic model

DriverFix should reason across layers rather than treating the installed driver version as the whole system:

1. **Hardware / PnP** — device identity, Hardware IDs, Compatible IDs, status, Problem Code.
2. **Installed package** — provider, version, date, INF, signature state.
3. **Windows applicability** — OS/build/architecture and supported package applicability.
4. **Component stack** — extensions, services and device-class-specific components.
5. **Runtime behavior** — whether the actual function works before and after repair.
6. **Recovery state** — verified backup, transaction journal, reboot/crash recovery and rollback evidence.

## Device coverage

### Core Windows hardware

- chipset and motherboard/system devices;
- ACPI, SMBus, PCI/PCIe, GPIO, I²C and Serial IO where applicable;
- USB controllers and hubs;
- SATA/NVMe/storage controllers;
- Ethernet, Wi-Fi and Bluetooth;
- GPU and associated driver components;
- webcams and other PnP peripherals.

### Audio

Audio is a first-class diagnostic domain, not simply a version check:

- onboard codec/audio controller;
- USB audio;
- Bluetooth audio;
- HDMI/DisplayPort audio;
- playback endpoints: speakers/headphones/headsets;
- capture endpoints: microphones/headsets;
- Windows Audio / endpoint initialization;
- OEM extension INF/APO/component presence where evidence is available;
- power/restart/redetection behavior.

## Canonical Windows 11 audio acceptance case

Symptom:

1. headphones are physically connected before/at Windows startup;
2. Windows starts but the expected audio endpoint/sound is unavailable;
3. unplugging and reconnecting the headphones makes audio work.

DriverFix must not stop at `ProblemCode == 0` or “driver installed”. It should collect enough evidence to distinguish at least:

- PnP/device failure;
- driver/package mismatch;
- Windows-build/package applicability problem;
- endpoint enumeration/initialization problem;
- audio service/component issue;
- OEM extension/APO stack issue where observable;
- power/redetection state problem.

A repair is VERIFIED only when the original behavioral symptom no longer reproduces under the defined test, not merely because installation returned success.

## Safety boundary

- no name/manufacturer substring guessing for compatibility;
- no repair before compatibility evidence and verified backup;
- no `exit code 0 = success` shortcut;
- no destructive force/delete fallback merely to beat Windows ranking;
- no automatic replay of ambiguous mutation after crash/reboot;
- firmware/BIOS may be diagnosed initially, but automated flashing is out of scope until a separate high-assurance design exists.

## Product promise

> Find the actual driver-related problem, explain it, repair the smallest safe layer, and prove that the device now works.
