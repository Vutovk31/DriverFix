namespace DriverFix.Core.Rollback;

public enum RollbackOutcome
{
    Blocked,
    RestoreRejected,
    AwaitingSystemReboot,
    RolledBack,
    ManualRecoveryRequired
}
