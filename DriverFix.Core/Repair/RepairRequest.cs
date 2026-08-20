using DriverFix.Core.Backup;
using DriverFix.Core.Models;

namespace DriverFix.Core.Repair;

public sealed record RepairRequest(
    string TransactionId,
    string TargetInstanceId,
    string CandidateInfPath,
    bool CompatibilityVerified,
    int DriverFixScore,
    int ConnectedMatchingDeviceCount,
    DriverBackupVerificationResult Backup,
    DeviceSnapshot BeforeSnapshot
);

public sealed record RepairResult(
    RepairOutcome Outcome,
    string Evidence,
    DeviceSnapshot? AfterSnapshot = null,
    bool RebootRequired = false
);
