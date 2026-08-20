using DriverFix.Core.Backup;

namespace DriverFix.Core.Abstractions;

public interface IDriverBackupService
{
    Task<DriverBackupVerificationResult> ExportExactInfAsync(
        string infName,
        string targetDirectory,
        CancellationToken cancellationToken = default);
}
