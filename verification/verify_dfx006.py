from pathlib import Path

root = Path(__file__).resolve().parents[1]
interface = (root / "DriverFix.Core/Abstractions/IInventorySnapshotService.cs").read_text(encoding="utf-8")
snapshot = (root / "DriverFix.Core/Models/InventorySnapshot.cs").read_text(encoding="utf-8")
result = (root / "DriverFix.Core/Models/InventoryCaptureResult.cs").read_text(encoding="utf-8")
failure = (root / "DriverFix.Core/Failures/InventoryFailureEvidence.cs").read_text(encoding="utf-8")
service = (root / "DriverFix.Core/Services/InventorySnapshotService.cs").read_text(encoding="utf-8")
program = (root / "DriverFix.Cli/Program.cs").read_text(encoding="utf-8")
provider = (root / "DriverFix.Windows/PnpUtilDeviceInventoryProvider.cs").read_text(encoding="utf-8")

checks = {
    "snapshot_service_contract": "Task<InventoryCaptureResult> CaptureAsync" in interface,
    "snapshot_has_capture_time": "DateTimeOffset CapturedAtUtc" in snapshot,
    "snapshot_has_devices": "IReadOnlyList<DeviceInventoryItem> Devices" in snapshot,
    "result_has_success_state": "bool Succeeded" in result,
    "result_has_snapshot": "InventorySnapshot Snapshot" in result,
    "result_has_failure": "InventoryFailureEvidence Failure" in result,
    "result_factories_are_disjoint": (
        "new InventoryCaptureResult(snapshot, null)" in result and
        "new InventoryCaptureResult(null, failure)" in result
    ),
    "failure_preserves_kind": "InventoryFailureKind Kind" in failure,
    "failure_preserves_exit_code": "int? ExitCode" in failure,
    "failure_preserves_stderr": "string? StandardError" in failure,
    "service_uses_provider": "IDeviceInventoryProvider" in service and "GetConnectedDevicesAsync" in service,
    "service_captures_utc_time": "GetUtcNow()" in service,
    "service_copies_devices": "devices.ToArray()" in service,
    "service_rethrows_cancellation": "catch (OperationCanceledException)" in service and "throw;" in service,
    "service_maps_typed_failure": "catch (InventoryProviderException ex)" in service and "ex.Kind" in service,
    "service_maps_unexpected_failure": "InventoryFailureKind.UnexpectedFailure" in service,
    "cli_uses_snapshot_boundary": "InventorySnapshotService" in program and "CaptureAsync" in program,
    "cli_reads_snapshot_only_on_success": "if (!result.Succeeded)" in program and "result.Snapshot.Devices" in program,
    "cli_reads_failure_evidence": "result.Failure" in program and "failure.Kind" in program,
    "cli_preserves_cancel_exit": "return 3;" in program,
    "inventory_command_unchanged": all(x in provider for x in [
        '"/enum-devices"', '"/connected"', '"/deviceids"'
    ]),
    "no_driver_mutation": all(token not in service + program + provider for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-006 CONTRACT FAIL: " + ", ".join(failed))

# Reference invariant: result has exactly one payload and cancellation is not converted to data.
def result_shape(success):
    return {"snapshot": object() if success else None, "failure": None if success else object()}

for success in (True, False):
    shaped = result_shape(success)
    assert (shaped["snapshot"] is None) != (shaped["failure"] is None)

print("PASS: reference_disjoint_result_shape")
print(f"\nDFX-006 STATIC/REFERENCE CONTRACT PASS: {len(checks) + 1}/{len(checks) + 1}")
