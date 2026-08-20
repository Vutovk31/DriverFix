namespace DriverFix.Core.Backup;

public sealed record DriverBackupVerificationResult(
    string InfName,
    string TargetDirectory,
    bool IsVerified,
    IReadOnlyList<string> ExportedFiles,
    long TotalBytes,
    string Evidence
);
