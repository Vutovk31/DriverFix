from pathlib import Path

root = Path(__file__).resolve().parents[1]
phase = (root / "DriverFix.Core/Transactions/TransactionPhase.cs").read_text(encoding="utf-8")
planner = (root / "DriverFix.Core/Transactions/TransactionRecoveryPlanner.cs").read_text(encoding="utf-8")
entry = (root / "DriverFix.Core/Transactions/TransactionJournalEntry.cs").read_text(encoding="utf-8")
journal = (root / "DriverFix.Persistence/JsonTransactionJournal.cs").read_text(encoding="utf-8")
priv = (root / "DriverFix.Windows/Security/WindowsPrivilegeBoundary.cs").read_text(encoding="utf-8")

checks = {
    "phases_present": all(x in phase for x in [
        "MutationStarted", "MutationApplied", "AwaitingReboot", "VerificationStarted",
        "Verified", "RollbackStarted", "RollbackApplied", "RollbackAwaitingReboot",
        "RollbackVerificationStarted", "RolledBack", "ManualRecoveryRequired"
    ]),
    "resume_verification_only": "ResumeVerification" in planner and "Do not replay install mutation" in planner,
    "resume_rollback_verification_only": "ResumeRollbackVerification" in planner and "Do not replay restore mutation" in planner,
    "ambiguous_mutation_manual": "TransactionPhase.MutationStarted" in planner and "ManualRecoveryRequired" in planner,
    "journal_has_recovery_fields": all(x in entry for x in [
        "TransactionId", "TargetInstanceId", "Phase", "OriginalInfName", "OriginalDriverVersion",
        "CandidateInfPath", "BackupDirectory", "RebootRequired", "UpdatedUtc", "Detail"
    ]),
    "temp_write": 'path + ".tmp"' in journal,
    "write_through": "FileOptions.WriteThrough" in journal,
    "flush_to_disk": "Flush(flushToDisk: true)" in journal,
    "atomic_replace_or_move": "File.Replace" in journal and "File.Move" in journal,
    "terminal_entries_ignored": "TransactionPhase.Verified or TransactionPhase.RolledBack" in journal,
    "corrupt_not_replayed": "catch (JsonException)" in journal,
    "path_containment": "Path.GetFileName(fileName)" in journal and "escaped its configured root" in journal,
    "windows_privilege_check": all(x in priv for x in [
        "WindowsIdentity.GetCurrent", "WindowsPrincipal", "WindowsBuiltInRole.Administrator"
    ]),
    "no_auto_elevation": all(x not in priv for x in ["runas", "Process.Start", "Verb ="]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("DFX-014 CONTRACT FAIL: " + ", ".join(failed))

print(f"\nDFX-014 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
