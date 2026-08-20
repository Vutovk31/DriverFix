namespace DriverFix.Core.Failures;

public enum InventoryFailureKind
{
    PlatformUnsupported,
    ProcessLaunchFailed,
    ToolReturnedNonZero,
    UnexpectedFailure
}
