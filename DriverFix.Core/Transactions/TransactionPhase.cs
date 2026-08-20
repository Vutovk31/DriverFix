namespace DriverFix.Core.Transactions;

public enum TransactionPhase
{
    Created,
    BeforeStateCaptured,
    BackupVerified,
    MutationStarted,
    MutationApplied,
    AwaitingReboot,
    VerificationStarted,
    Verified,
    RollbackRequested,
    RollbackStarted,
    RollbackApplied,
    RollbackAwaitingReboot,
    RollbackVerificationStarted,
    RolledBack,
    ManualRecoveryRequired,
    Failed
}
