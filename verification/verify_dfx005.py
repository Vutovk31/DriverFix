from pathlib import Path

root = Path(__file__).resolve().parents[1]
kind = (root / "DriverFix.Core/Failures/InventoryFailureKind.cs").read_text(encoding="utf-8")
exc = (root / "DriverFix.Core/Failures/InventoryProviderException.cs").read_text(encoding="utf-8")
provider = (root / "DriverFix.Windows/PnpUtilDeviceInventoryProvider.cs").read_text(encoding="utf-8")
program = (root / "DriverFix.Cli/Program.cs").read_text(encoding="utf-8")

checks = {
    "typed_failure_kinds": all(x in kind for x in [
        "PlatformUnsupported", "ProcessLaunchFailed", "ToolReturnedNonZero", "UnexpectedFailure"
    ]),
    "exception_carries_kind": "InventoryFailureKind Kind" in exc,
    "exception_carries_exit_code": "int? ExitCode" in exc,
    "exception_carries_stderr": "string? StandardError" in exc,
    "platform_is_typed": "InventoryFailureKind.PlatformUnsupported" in provider,
    "process_failure_is_typed": "InventoryFailureKind.ProcessLaunchFailed" in provider,
    "nonzero_is_typed": "InventoryFailureKind.ToolReturnedNonZero" in provider,
    "unexpected_is_typed": "InventoryFailureKind.UnexpectedFailure" in provider,
    "exit_code_preserved": "result.ExitCode" in provider,
    "stderr_bounded": "Limit(result.StandardError, 4096)" in provider,
    "cancellation_rethrows": "catch (OperationCanceledException)" in provider and "throw;" in provider,
    "cli_surfaces_typed_kind": "InventoryProviderException" in program and "ex.Kind" in program,
    "cli_has_cancel_exit": "return 3;" in program,
    "inventory_command_unchanged": all(x in provider for x in [
        '"/enum-devices"', '"/connected"', '"/deviceids"'
    ]),
    "no_driver_mutation": all(token not in provider + program for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-005 CONTRACT FAIL: " + ", ".join(failed))

# Reference policy: cancellation stays cancellation, not a provider failure.
def classify(platform_ok=True, launch_ok=True, exit_code=0, parse_ok=True):
    if not platform_ok:
        return "PlatformUnsupported"
    if not launch_ok:
        return "ProcessLaunchFailed"
    if exit_code != 0:
        return "ToolReturnedNonZero"
    if not parse_ok:
        return "UnexpectedFailure"
    return "Success"

assert classify(platform_ok=False) == "PlatformUnsupported"
assert classify(launch_ok=False) == "ProcessLaunchFailed"
assert classify(exit_code=5) == "ToolReturnedNonZero"
assert classify(parse_ok=False) == "UnexpectedFailure"
assert classify() == "Success"
print("PASS: reference_failure_classification")

print(f"\nDFX-005 STATIC/REFERENCE CONTRACT PASS: {len(checks) + 1}/{len(checks) + 1}")
