using DriverFix.Core.Abstractions;
using DriverFix.Core.Models;

namespace DriverFix.Windows;

public sealed class PowerShellDriverMetadataProvider : IDriverMetadataProvider
{
    private const string Script =
        "Get-CimInstance Win32_PnPSignedDriver | " +
        "Select-Object DeviceID,DeviceName,DriverProviderName,DriverVersion,DriverDate,InfName,IsSigned,Signer | " +
        "ConvertTo-Json -Compress -Depth 3";

    private readonly IProcessRunner _processRunner;

    public PowerShellDriverMetadataProvider(IProcessRunner processRunner)
    {
        _processRunner = processRunner ??
            throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<IReadOnlyList<DriverMetadata>> GetInstalledDriversAsync(
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Installed driver metadata requires Windows.");

        cancellationToken.ThrowIfCancellationRequested();

        ProcessResult result;
        try
        {
            result = await _processRunner.RunAsync(
                "powershell.exe",
                new[] { "-NoProfile", "-NonInteractive", "-Command", Script },
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"Win32_PnPSignedDriver query failed with exit code {result.ExitCode}: {Limit(result.StandardError, 4096)}");

        return DriverMetadataJsonParser.Parse(result.StandardOutput)
            .Where(item => !string.IsNullOrWhiteSpace(item.DeviceId))
            .ToArray();
    }

    private static string Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
