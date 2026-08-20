from pathlib import Path

root = Path(__file__).resolve().parents[1]
executor = (root / "DriverFix.Windows/PnpUtilRepairExecutor.cs").read_text(encoding="utf-8")
verify = (root / "DriverFix.Core/Repair/RepairVerificationService.cs").read_text(encoding="utf-8")
request = (root / "DriverFix.Core/Repair/RepairRequest.cs").read_text(encoding="utf-8")

checks = {
    "backup_required": "request.Backup.IsVerified" in executor and "request.Backup.TotalBytes <= 0" in executor,
    "compatibility_required": "request.CompatibilityVerified" in executor and "request.DriverFixScore <= 0" in executor,
    "blast_radius_gate": "request.ConnectedMatchingDeviceCount != 1" in executor,
    "before_target_bound": "request.BeforeSnapshot.Device.InstanceId" in executor,
    "exact_inf_only": "Path.GetExtension(request.CandidateInfPath)" in executor,
    "wildcards_blocked": "IndexOf('*')" in executor and "IndexOf('?')" in executor,
    "inf_must_exist": "File.Exists(fullInf)" in executor,
    "install_shape": 'new[] { "/add-driver", fullInf, "/install" }' in executor,
    "targeted_restart": 'new[] { "/restart-device", request.TargetInstanceId }' in executor,
    "reboot_codes": "3010 or 1641" in executor,
    "install_rejected_259": "install.ExitCode == 259" in executor,
    "unknown_mutation_manual": "ManualRecoveryRequired" in executor,
    "post_snapshot": "_snapshotReader.ReadAsync" in executor,
    "verification_same_target": "before.Device.InstanceId" in verify and "after.Device.InstanceId" in verify,
    "verification_health": "after.Device.ProblemCode is > 0" in verify,
    "verification_meaningful_change": "identityChanged || problemCleared" in verify,
    "no_destructive_fallback": all(token not in executor for token in ["/delete-driver", "/remove-device", "/uninstall", "/force", "/subdirs"]),
    "request_has_count": "ConnectedMatchingDeviceCount" in request,
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("DFX-012 CONTRACT FAIL: " + ", ".join(failed))
print(f"\nDFX-012 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
