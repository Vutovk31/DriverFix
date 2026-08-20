namespace DriverFix.Core.Transactions;

public sealed record TransactionJournalEntry(
    string TransactionId,
    string TargetInstanceId,
    TransactionPhase Phase,
    string? OriginalInfName,
    string? OriginalDriverVersion,
    string? CandidateInfPath,
    string? BackupDirectory,
    bool RebootRequired,
    DateTimeOffset UpdatedUtc,
    string? Detail
);
