namespace DriverFix.Core.Transactions;

public static class TransactionRecoveryPlanner
{
    public static RecoveryDecision Decide(TransactionPhase phase) => phase switch
    {
        TransactionPhase.Verified or TransactionPhase.RolledBack =>
            new(RecoveryAction.None, "Terminal verified state requires no recovery action."),

        TransactionPhase.MutationApplied or
        TransactionPhase.AwaitingReboot or
        TransactionPhase.VerificationStarted =>
            new(RecoveryAction.ResumeVerification,
                "Mutation is known applied; resume verification only. Do not replay install mutation."),

        TransactionPhase.RollbackApplied or
        TransactionPhase.RollbackAwaitingReboot or
        TransactionPhase.RollbackVerificationStarted =>
            new(RecoveryAction.ResumeRollbackVerification,
                "Rollback mutation is known applied; resume rollback verification only. Do not replay restore mutation."),

        TransactionPhase.MutationStarted or TransactionPhase.RollbackStarted =>
            new(RecoveryAction.ManualRecoveryRequired,
                "Mutation boundary is ambiguous after interruption; blind replay is forbidden."),

        TransactionPhase.Created or
        TransactionPhase.BeforeStateCaptured or
        TransactionPhase.BackupVerified =>
            new(RecoveryAction.None, "No repair mutation has been applied."),

        _ => new(RecoveryAction.ManualRecoveryRequired,
            "State is not safely resumable by an automatic mutation path.")
    };
}
