namespace DriverFix.Core.Failures;

public sealed class InventoryProviderException : Exception
{
    public InventoryProviderException(
        InventoryFailureKind kind,
        string message,
        int? exitCode = null,
        string? standardError = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
        ExitCode = exitCode;
        StandardError = standardError;
    }

    public InventoryFailureKind Kind { get; }

    public int? ExitCode { get; }

    public string? StandardError { get; }
}
