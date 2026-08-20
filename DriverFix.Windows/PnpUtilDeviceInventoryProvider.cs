using DriverFix.Core.Abstractions;
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
            throw new PlatformNotSupportedException(
                "PnPUtil hardware inventory requires Windows.");

        cancellationToken.ThrowIfCancellationRequested();

        var result = await _processRunner.RunAsync(
            "pnputil.exe",
            Arguments,
            cancellationToken);

        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"PnPUtil inventory failed with exit code {result.ExitCode}: " +
                Limit(result.StandardError, 4096));

        return PnpUtilInventoryParser.Parse(result.StandardOutput);
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
