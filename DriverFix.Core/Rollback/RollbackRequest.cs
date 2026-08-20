using DriverFix.Core.Backup;
using DriverFix.Core.Models;

namespace DriverFix.Core.Rollback;

public sealed record RollbackRequest(
    string TransactionId,
    string TargetInstanceId,
    string BackupInfPath,
    int ConnectedMatchingDeviceCount,
    DriverBackupVerificationResult Backup,
    DeviceSnapshot OriginalSnapshot,
    DeviceSnapshot FailedSnapshot
);

public sealed record RollbackResult(
    RollbackOutcome Outcome,
    string Evidence,
    DeviceSnapshot? AfterSnapshot = null,
    bool RebootRequired = false
);
