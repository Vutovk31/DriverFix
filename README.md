# DriverFix

**DriverFix is a Windows diagnostics and repair utility for driver-related hardware problems.**

DriverFix does **not** exist to blindly install the newest driver. Its job is to identify a concrete Windows ↔ hardware ↔ driver problem, collect evidence, select a compatible repair, back up the current state, apply the smallest safe change, verify that the original symptom is gone, and roll back when verification fails.

## Product loop

`detect → diagnose → match → backup → repair → verify → rollback`

## What DriverFix should diagnose

- motherboard/chipset, ACPI and system devices;
- USB controllers and hubs;
- storage controllers;
- LAN, Wi-Fi and Bluetooth;
- GPU and related components;
- onboard, USB, Bluetooth and HDMI/DisplayPort audio;
- speakers, headphones, headsets and microphones;
- webcams and other PnP devices.

BIOS/UEFI/firmware may be diagnosed, but automatic firmware flashing is outside the initial safety boundary.

## Compatibility, not version chasing

A device can be green in Device Manager and still be broken after a Windows update. DriverFix therefore evaluates more than the installed version: PnP state, hardware identifiers, driver package, Windows build, relevant services/components, observable device behavior, and post-repair behavior.

One canonical acceptance case is Windows 11 audio where headphones are already connected but sound appears only after unplug/replug. The correct outcome is not merely “driver installed”; DriverFix should identify the failing layer and verify that endpoint initialization works after repair.

## Safety principles

- evidence before mutation;
- exact device/package targeting where Windows permits it;
- no speculative compatibility from names or substrings;
- verified backup before repair;
- no destructive fallback merely to force an older/lower-ranked driver;
- success requires observed post-state, not only exit code 0;
- crash/reboot recovery must never blindly replay an ambiguous mutation.

## Development

GitHub `main` is the canonical physical source of truth. Development is hybrid: automated cycles and manual user commits share the same repository and are reconciled before each new cycle.

See:

- [`docs/CONCEPT.md`](docs/CONCEPT.md)
- [`docs/ROADMAP.md`](docs/ROADMAP.md)
- [`docs/HYBRID_WORKFLOW.md`](docs/HYBRID_WORKFLOW.md)
- [`docs/STATUS.md`](docs/STATUS.md)

## License

MIT.
