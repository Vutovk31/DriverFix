namespace DriverFix.Core.Failures;

public sealed record InventoryFailureEvidence(
    InventoryFailureKind Kind,
    string Message,
    int? ExitCode,
    string? StandardError
);
