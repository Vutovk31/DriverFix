using DriverFix.Core.Abstractions;
using DriverFix.Core.Failures;
using DriverFix.Core.Models;

namespace DriverFix.Windows;

public sealed class PnpUtilDeviceInventoryProvider : IDeviceInventoryProvider
{
    private static readonly string[] Arguments =
    {
        "/enum-devices",
        "/connected",
        "/deviceids"
    };

    private readonly IProcessRunner _processRunner;

    public PnpUtilDeviceInventoryProvider(IProcessRunner processRunner)
    {
        _processRunner = processRunner ??
            throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<IReadOnlyList<DeviceInventoryItem>> GetConnectedDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new InventoryProviderException(
                InventoryFailureKind.PlatformUnsupported,
                "PnPUtil hardware inventory requires Windows.");

        cancellationToken.ThrowIfCancellationRequested();

        ProcessResult result;
        try
        {
            result = await _processRunner.RunAsync(
                "pnputil.exe",
                Arguments,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InventoryProviderException(
                InventoryFailureKind.ProcessLaunchFailed,
                $"Could not execute PnPUtil inventory: {ex.Message}",
                innerException: ex);
        }

        if (result.ExitCode != 0)
        {
            var stderr = Limit(result.StandardError, 4096);
            throw new InventoryProviderException(
                InventoryFailureKind.ToolReturnedNonZero,
                $"PnPUtil inventory failed with exit code {result.ExitCode}.",
                result.ExitCode,
                stderr);
        }

        try
        {
            return PnpUtilInventoryParser.Parse(result.StandardOutput);
        }
        catch (Exception ex)
        {
            throw new InventoryProviderException(
                InventoryFailureKind.UnexpectedFailure,
                $"PnPUtil inventory output could not be parsed: {ex.Message}",
                innerException: ex);
        }
    }

    private static string Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength];
    }
}
