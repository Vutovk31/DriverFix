using DriverFix.Core.Backup;
using DriverFix.Core.Models;
using DriverFix.Core.Repair;

namespace DriverFix.Cli;

internal static class RepairPreflightSmoke
{
    public static int Run()
    {
        const string instanceId = "ROOT\\DRIVERFIX_PREFLIGHT\\0000";
        var device = new DeviceInventoryItem(
            instanceId,
            "DriverFix preflight sentinel",
            "System",
            "DriverFix",
            "Problem",
            28,
            new[] { "ROOT\\DRIVERFIX_PREFLIGHT" },
            Array.Empty<string>());
        var backup = new DriverBackupVerificationResult(
            "oem0.inf",
            Path.GetTempPath(),
            IsVerified: false,
            ExportedFiles: Array.Empty<string>(),
            TotalBytes: 0,
            Evidence: "Synthetic unverified backup sentinel.");
        var request = new RepairRequest(
            "preflight-smoke",
            instanceId,
            "never-reached.inf",
            CompatibilityVerified: true,
            DriverFixScore: 100,
            ConnectedMatchingDeviceCount: 1,
            backup,
            new DeviceSnapshot(device, null));

        var result = RepairPreflightService.Evaluate(request);
        const string expected = "Verified driver backup is required before repair.";
        if (result.IsAllowed || !string.Equals(result.Evidence, expected, StringComparison.Ordinal))
        {
            Console.Error.WriteLine($"Repair preflight negative smoke failed: allowed={result.IsAllowed}; evidence={result.Evidence}");
            return 1;
        }

        Console.WriteLine("Repair preflight negative smoke: BLOCKED");
        Console.WriteLine(result.Evidence);
        Console.WriteLine("No ProcessRunner or PnPUtil operation is reachable from this smoke path.");
        return 0;
    }
}
