from pathlib import Path

root = Path(__file__).resolve().parents[1]
project = (root / "DriverFix.Cli/DriverFix.Cli.csproj").read_text(encoding="utf-8")
program = (root / "DriverFix.Cli/Program.cs").read_text(encoding="utf-8")
formatter = (root / "DriverFix.Cli/DeviceInventoryTextFormatter.cs").read_text(encoding="utf-8")

checks = {
    "windows_cli_project": "<TargetFramework>net10.0-windows</TargetFramework>" in project,
    "references_core": "DriverFix.Core.csproj" in project,
    "references_windows": "DriverFix.Windows.csproj" in project,
    "uses_existing_provider": "PnpUtilDeviceInventoryProvider" in program,
    "uses_existing_process_runner": "new ProcessRunner()" in program,
    "calls_inventory_boundary": "CaptureAsync" in program,
    "prints_formatter_output": "DeviceInventoryTextFormatter.Format(result.Snapshot.Devices)" in program,
    "nonwindows_exit_code": "return 2;" in program,
    "failure_exit_code": "return 1;" in program,
    "success_exit_code": "return 0;" in program,
    "formatter_device_count": "Connected devices: {devices.Count}" in formatter,
    "formatter_instance_id": "Instance ID:" in formatter,
    "formatter_problem_code": "Problem Code:" in formatter,
    "formatter_hardware_ids": '"Hardware IDs"' in formatter,
    "formatter_compatible_ids": '"Compatible IDs"' in formatter,
    "formatter_preserves_order": "for (var index = 0; index < devices.Count; index++)" in formatter,
    "no_driver_mutation": all(token not in (program + formatter) for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-003 CONTRACT FAIL: " + ", ".join(failed))

print(f"\nDFX-003 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
