from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
service = (root / "DriverFix.Windows/PnpUtilDriverBackupService.cs").read_text(encoding="utf-8")
contract = (root / "DriverFix.Core/Abstractions/IDriverBackupService.cs").read_text(encoding="utf-8")
result = (root / "DriverFix.Core/Backup/DriverBackupVerificationResult.cs").read_text(encoding="utf-8")

checks = {
    "backup_interface": "ExportExactInfAsync" in contract,
    "verified_result": all(x in result for x in ["IsVerified", "ExportedFiles", "TotalBytes", "Evidence"]),
    "exact_oem_inf_only": "^oem[0-9]+\\\\.inf$" in service,
    "exact_export_command": 'new[] { "/export-driver", exactInf, fullTarget }' in service,
    "no_wildcard_export": '"*"' not in service.split('RunAsync', 1)[1].split('cancellationToken', 1)[0],
    "empty_target_gate": "Directory.EnumerateFileSystemEntries(fullTarget).Any()" in service,
    "nonzero_exit_blocks": "result.ExitCode != 0" in service,
    "inf_presence_gate": "infFiles.Length == 0" in service,
    "zero_length_gate": "FileInfo(path).Length <= 0" in service,
    "disk_evidence": "TotalBytes" in result and "verified on disk" in service,
    "no_repair_mutation": all(token not in service for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install", "/uninstall"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-011 CONTRACT FAIL: " + ", ".join(failed))

pattern = re.compile(r"^oem[0-9]+\.inf$", re.IGNORECASE)
assert pattern.fullmatch("oem42.inf")
assert pattern.fullmatch("OEM0.INF")
assert not pattern.fullmatch("*.inf")
assert not pattern.fullmatch("net.inf")
assert not pattern.fullmatch("oem42.inf.bak")
assert not pattern.fullmatch("../oem42.inf")
print("PASS: exact_inf_reference_behavior")

print(f"\nDFX-011 STATIC/REFERENCE CONTRACT PASS: {len(checks) + 1}/{len(checks) + 1}")
