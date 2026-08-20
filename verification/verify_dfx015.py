from pathlib import Path

root = Path(__file__).resolve().parents[1]
contract = (root / "DriverFix.Core/Elevation/ElevatedOperation.cs").read_text(encoding="utf-8")
validator = (root / "DriverFix.Windows/Elevation/ElevatedOperationValidator.cs").read_text(encoding="utf-8")
broker = (root / "DriverFix.Windows/Elevation/ElevatedWorkerBroker.cs").read_text(encoding="utf-8")
worker = (root / "DriverFix.Elevated/Program.cs").read_text(encoding="utf-8")
manifest = (root / "DriverFix.Elevated/app.manifest").read_text(encoding="utf-8")
project = (root / "DriverFix.Elevated/DriverFix.Elevated.csproj").read_text(encoding="utf-8")

checks = {
    "three_allowed_operations": all(x in contract for x in ["InstallExactInf", "RestartExactDevice", "RestoreExactBackup"]),
    "typed_request": all(x in contract for x in ["ElevatedRequest", "ElevatedOperation", "Nonce"]),
    "current_user_pipe": "PipeOptions.CurrentUserOnly" in broker,
    "one_connection": "PipeDirection.InOut" in broker and "1," in broker,
    "random_nonce": "RandomNumberGenerator.GetBytes(32)" in broker,
    "uac_runas": 'Verb = "runas"' in broker and "UseShellExecute = true" in broker,
    "worker_requires_admin": 'level="requireAdministrator"' in manifest,
    "nonce_verified": "request.Nonce" in worker and "expectedNonce" in worker,
    "allow_list_validator": "Operation is not allowed." in validator,
    "exact_inf_validation": "Path.GetExtension" in validator and "Wildcards are not allowed" in validator,
    "worker_constructs_commands": all(x in worker for x in ["/add-driver", "/restart-device", "/install"]),
    "only_pnputil": 'runner.RunAsync("pnputil.exe"' in worker,
    "no_cmd_or_powershell": all(x not in worker + broker for x in ["cmd.exe", "powershell.exe", "pwsh.exe"]),
    "no_free_form_command_field": all(x not in contract for x in ["CommandText", "ShellCommand", "ArgumentsText", "Script"]),
    "separate_executable": "<OutputType>Exe</OutputType>" in project and "DriverFix.Elevated" in project,
    "reboot_codes_preserved": "3010 or 1641" in worker,
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("DFX-015 CONTRACT FAIL: " + ", ".join(failed))
print(f"\nDFX-015 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
