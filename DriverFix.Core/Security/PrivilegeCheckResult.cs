namespace DriverFix.Core.Security;

public sealed record PrivilegeCheckResult(
    bool IsWindows,
    bool IsElevated,
    string Evidence
);
