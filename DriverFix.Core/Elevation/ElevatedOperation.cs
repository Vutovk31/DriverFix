namespace DriverFix.Core.Elevation;

public enum ElevatedOperation
{
    InstallExactInf = 1,
    RestartExactDevice = 2,
    RestoreExactBackup = 3
}

public sealed record ElevatedRequest(
    string Nonce,
    ElevatedOperation Operation,
    string? InfPath = null,
    string? InstanceId = null
);

public sealed record ElevatedResponse(
    bool Accepted,
    int? ExitCode,
    bool RebootRequired,
    string Evidence
);
