namespace DriverFix.Core.Transactions;

public enum RecoveryAction
{
    None,
    ResumeVerification,
    ResumeRollbackVerification,
    ManualRecoveryRequired
}

public sealed record RecoveryDecision(RecoveryAction Action, string Evidence);
