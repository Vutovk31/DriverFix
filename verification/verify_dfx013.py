from pathlib import Path

root = Path(__file__).resolve().parents[1]
executor = (root / "DriverFix.Windows/PnpUtilRollbackExecutor.cs").read_text(encoding="utf-8")
verify = (root / "DriverFix.Core/Rollback/RollbackVerificationService.cs").read_text(encoding="utf-8")
request = (root / "DriverFix.Core/Rollback/RollbackRequest.cs").read_text(encoding="utf-8")

checks = {
    "verified_backup_required": "request.Backup.IsVerified" in executor and "request.Backup.TotalBytes <= 0" in executor,
    "blast_radius_gate": "request.ConnectedMatchingDeviceCount != 1" in executor,
    "snapshots_bound_to_target": "request.OriginalSnapshot.Device.InstanceId" in executor and "request.FailedSnapshot.Device.InstanceId" in executor,
    "exact_backup_inf_only": "Path.GetExtension(request.BackupInfPath)" in executor,
    "wildcards_blocked": "IndexOf('*')" in executor and "IndexOf('?')" in executor,
    "backup_inf_exists": "File.Exists(fullInf)" in executor,
    "restore_shape": 'new[] { "/add-driver", fullInf, "/install" }' in executor,
    "targeted_restart": 'new[] { "/restart-device", request.TargetInstanceId }' in executor,
    "reboot_codes": "3010 or 1641" in executor,
    "exit_259_manual": "restore.ExitCode == 259" in executor and "ManualRecoveryRequired" in executor,
    "unknown_mutation_manual": "Rollback mutation state is unknown" in executor,
    "post_snapshot": "_snapshotReader.ReadAsync" in executor,
    "same_target_verification": "original.Device.InstanceId" in verify and "after.Device.InstanceId" in verify,
    "healthy_after": "after.Device.ProblemCode is > 0" in verify,
    "original_identity_restored": "infRestored || versionRestored" in verify,
    "no_destructive_fallback": all(token not in executor for token in ["/delete-driver", "/remove-device", "/uninstall", "/force", "/subdirs"]),
    "request_has_matching_count": "ConnectedMatchingDeviceCount" in request,
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("DFX-013 CONTRACT FAIL: " + ", ".join(failed))
print(f"\nDFX-013 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
