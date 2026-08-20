using DriverFix.Core.Abstractions;
using DriverFix.Core.Candidates;

namespace DriverFix.Windows;

public sealed class WindowsUpdateDriverCandidateProvider : IDriverCandidateProvider
{
    private const string SearchCriteria =
        "Type='Driver' and IsInstalled=0 and IsHidden=0";

    private const string Script = @"
$session = New-Object -ComObject Microsoft.Update.Session
$searcher = $session.CreateUpdateSearcher()
$result = $searcher.Search(""Type='Driver' and IsInstalled=0 and IsHidden=0"")
$items = foreach ($u in $result.Updates) {
  [pscustomobject]@{
    UpdateId = $u.Identity.UpdateID
    Title = $u.Title
    DriverProvider = $u.DriverProvider
    DriverManufacturer = $u.DriverManufacturer
    DriverModel = $u.DriverModel
    DriverClass = $u.DriverClass
    DriverVerDate = $u.DriverVerDate
    DriverHardwareID = $u.DriverHardwareID
    IsDownloaded = $u.IsDownloaded
    IsHidden = $u.IsHidden
    EulaAccepted = $u.EulaAccepted
  }
}
$items | ConvertTo-Json -Compress -Depth 4
";

    private readonly IProcessRunner _processRunner;

    public WindowsUpdateDriverCandidateProvider(IProcessRunner processRunner)
    {
        _processRunner = processRunner ??
            throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<IReadOnlyList<DriverUpdateCandidate>> SearchAsync(
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Windows Update driver candidate discovery requires Windows.");

        cancellationToken.ThrowIfCancellationRequested();

        var result = await _processRunner.RunAsync(
            "powershell.exe",
            new[] { "-NoProfile", "-NonInteractive", "-Command", Script },
            cancellationToken);

        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"Windows Update driver search failed with exit code {result.ExitCode}: {Limit(result.StandardError, 4096)}");

        return DriverCandidateJsonParser.Parse(result.StandardOutput)
            .Where(candidate =>
                !string.IsNullOrWhiteSpace(candidate.UpdateId) &&
                !candidate.IsHidden)
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
