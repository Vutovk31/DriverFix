using System.Text.RegularExpressions;
using DriverFix.Core.Abstractions;
using DriverFix.Core.Backup;

namespace DriverFix.Windows;

public sealed class PnpUtilDriverBackupService : IDriverBackupService
{
    private static readonly Regex ExactOemInf = new(
        "^oem[0-9]+\\.inf$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IProcessRunner _processRunner;

    public PnpUtilDriverBackupService(IProcessRunner processRunner)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<DriverBackupVerificationResult> ExportExactInfAsync(
        string infName,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Driver backup export requires Windows.");

        if (string.IsNullOrWhiteSpace(infName) || !ExactOemInf.IsMatch(infName.Trim()))
            throw new ArgumentException("Backup requires an exact published OEM INF name such as oem42.inf.", nameof(infName));

        if (string.IsNullOrWhiteSpace(targetDirectory))
            throw new ArgumentException("Backup target directory is required.", nameof(targetDirectory));

        cancellationToken.ThrowIfCancellationRequested();

        var exactInf = infName.Trim();
        var fullTarget = Path.GetFullPath(targetDirectory);

        if (Directory.Exists(fullTarget) && Directory.EnumerateFileSystemEntries(fullTarget).Any())
            return Blocked(exactInf, fullTarget, "Backup target must be empty before export.");

        Directory.CreateDirectory(fullTarget);

        var result = await _processRunner.RunAsync(
            "pnputil.exe",
            new[] { "/export-driver", exactInf, fullTarget },
            cancellationToken);

        if (result.ExitCode != 0)
            return Blocked(
                exactInf,
                fullTarget,
                $"PnPUtil export failed with exit code {result.ExitCode}: {Limit(result.StandardError, 4096)}");

        var files = Directory.EnumerateFiles(fullTarget, "*", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var infFiles = files
            .Where(path => string.Equals(Path.GetExtension(path), ".inf", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (infFiles.Length == 0)
            return Blocked(exactInf, fullTarget, "PnPUtil returned success but no exported INF file exists.", files);

        var zeroLength = files.Where(path => new FileInfo(path).Length <= 0).ToArray();
        if (files.Length == 0 || zeroLength.Length > 0)
            return Blocked(exactInf, fullTarget, "Backup contains no files or contains zero-length files.", files);

        var totalBytes = files.Sum(path => new FileInfo(path).Length);
        return new(
            exactInf,
            fullTarget,
            true,
            files,
            totalBytes,
            $"Exact package {exactInf} exported successfully; {files.Length} files, {infFiles.Length} INF file(s), {totalBytes} bytes verified on disk.");
    }

    private static DriverBackupVerificationResult Blocked(
        string infName,
        string targetDirectory,
        string evidence,
        IReadOnlyList<string>? files = null) =>
        new(infName, targetDirectory, false, files ?? Array.Empty<string>(), 0, evidence);

    private static string Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
