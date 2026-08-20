from pathlib import Path

root = Path(__file__).resolve().parents[1]
provider = (root / "DriverFix.Windows/PnpUtilDeviceInventoryProvider.cs").read_text(encoding="utf-8")
parser = (root / "DriverFix.Windows/PnpUtilInventoryParser.cs").read_text(encoding="utf-8")
runner = (root / "DriverFix.Windows/ProcessRunner.cs").read_text(encoding="utf-8")
model = (root / "DriverFix.Core/Models/DeviceInventoryItem.cs").read_text(encoding="utf-8")

checks = {
    "device_model_has_identity": "string InstanceId" in model,
    "device_model_has_problem_code": "int? ProblemCode" in model,
    "device_model_has_hardware_ids": "HardwareIds" in model,
    "device_model_has_compatible_ids": "CompatibleIds" in model,
    "provider_enum_devices": '"/enum-devices"' in provider,
    "provider_connected_only": '"/connected"' in provider,
    "provider_deviceids": '"/deviceids"' in provider,
    "provider_windows_guard": "OperatingSystem.IsWindows()" in provider,
    "provider_exit_code_guard": "result.ExitCode != 0" in provider,
    "runner_uses_argument_list": "ArgumentList.Add" in runner,
    "runner_propagates_cancellation": "catch (OperationCanceledException)" in runner and "throw;" in runner,
    "parser_supports_problem_code": "ParseProblemCode" in parser,
    "parser_supports_ru_instance_id": "Идентификатор экземпляра" in parser,
    "no_driver_mutation": all(token not in provider + runner + parser for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", '"/install"'
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-001 CONTRACT FAIL: " + ", ".join(failed))

print(f"\nDFX-001 CONTRACT PASS: {len(checks)}/{len(checks)}")
