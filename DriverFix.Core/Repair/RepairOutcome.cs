namespace DriverFix.Core.Repair;

public enum RepairOutcome
{
    Blocked,
    InstallRejected,
    AwaitingSystemReboot,
    Verified,
    RollbackRequired,
    ManualRecoveryRequired
}
