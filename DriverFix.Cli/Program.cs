using System.Text.RegularExpressions;
using DriverFix.Core.Elevation;
using DriverFix.Core.Failures;
using DriverFix.Core.Services;
using DriverFix.Windows;
using DriverFix.Windows.Elevation;

namespace DriverFix.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && string.Equals(args[0], "--elevation-smoke", StringComparison.Ordinal))
                return await RunElevationSmokeAsync();

            if (args.Length == 1 && string.Equals(args[0], "--driver-metadata-smoke", StringComparison.Ordinal))
                return await RunDriverMetadataSmokeAsync();

            if (args.Length == 1 && string.Equals(args[0], "--diagnosis-smoke", StringComparison.Ordinal))
                return await DiagnosisSmoke.RunAsync();

            if (args.Length == 1 && string.Equals(args[0], "--wua-candidate-smoke", StringComparison.Ordinal))
                return await RunWindowsUpdateCandidateSmokeAsync();

            if (args.Length == 1 && string.Equals(args[0], "--workstation-readonly-smoke", StringComparison.Ordinal))
                return await RunWorkstationReadOnlySmokeAsync();

            if (args.Length == 1 && string.Equals(args[0], "--backup-export-smoke", StringComparison.Ordinal))
                return await RunBackupExportSmokeAsync();

            if (args.Length == 1 && string.Equals(args[0], "--repair-preflight-smoke", StringComparison.Ordinal))
                return RepairPreflightSmoke.Run();

            if (args.Length != 0)
            {
                Console.Error.WriteLine("Usage: DriverFix.exe [--elevation-smoke|--driver-metadata-smoke|--diagnosis-smoke|--wua-candidate-smoke|--workstation-readonly-smoke|--backup-export-smoke|--repair-preflight-smoke]");
                return 64;
            }

            return await InteractiveReadOnlyRun.RunAsync();
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("DriverFix operation cancelled.");
            return 3;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DriverFix failed: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunInventoryAsync()
    {
        var provider = new PnpUtilDeviceInventoryProvider(new ProcessRunner());
        var snapshotService = new InventorySnapshotService(provider);
        var result = await snapshotService.CaptureAsync();

        if (!result.Succeeded)
        {
            var failure = result.Failure;
            Console.Error.WriteLine(
                $"DriverFix inventory failed [{failure.Kind}]: {failure.Message}");

            return failure.Kind == InventoryFailureKind.PlatformUnsupported
                ? 2
                : 1;
        }

        Console.WriteLine(
            DeviceInventoryTextFormatter.Format(result.Snapshot.Devices));
        return 0;
    }

    private static async Task<int> RunDriverMetadataSmokeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix driver metadata smoke requires Windows.");
            return 2;
        }

        var provider = new PowerShellDriverMetadataProvider(new ProcessRunner());
        var drivers = await provider.GetInstalledDriversAsync();

        if (drivers.Count == 0)
        {
            Console.Error.WriteLine("DriverFix driver metadata smoke returned no installed driver records.");
            return 1;
        }

        Console.WriteLine($"Installed driver metadata: {drivers.Count}");
        return 0;
    }

    private static async Task<int> RunWindowsUpdateCandidateSmokeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix WUA candidate smoke requires Windows.");
            return 2;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var provider = new WindowsUpdateDriverCandidateProvider(new ProcessRunner());
        var candidates = await provider.SearchAsync(timeout.Token);

        Console.WriteLine($"Windows Update driver candidates: {candidates.Count}");
        return 0;
    }

    private static async Task<int> RunWorkstationReadOnlySmokeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix workstation read-only smoke requires Windows.");
            return 2;
        }

        var processRunner = new ProcessRunner();
        var inventoryProvider = new PnpUtilDeviceInventoryProvider(processRunner);
        var snapshotService = new InventorySnapshotService(inventoryProvider);
        var inventory = await snapshotService.CaptureAsync();

        if (!inventory.Succeeded)
        {
            var failure = inventory.Failure;
            Console.Error.WriteLine(
                $"DriverFix workstation read-only smoke inventory failed [{failure.Kind}]: {failure.Message}");
            return 1;
        }

        var metadataProvider = new PowerShellDriverMetadataProvider(processRunner);
        var drivers = await metadataProvider.GetInstalledDriversAsync();
        if (drivers.Count == 0)
        {
            Console.Error.WriteLine("DriverFix workstation read-only smoke returned no installed driver records.");
            return 1;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var candidateProvider = new WindowsUpdateDriverCandidateProvider(processRunner);
        var candidates = await candidateProvider.SearchAsync(timeout.Token);

        Console.WriteLine("Workstation read-only smoke: PASS");
        Console.WriteLine($"Connected devices: {inventory.Snapshot.Devices.Count}");
        Console.WriteLine($"Installed driver metadata: {drivers.Count}");
        Console.WriteLine($"Windows Update driver candidates: {candidates.Count}");
        return 0;
    }

    private static async Task<int> RunBackupExportSmokeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix backup export smoke requires Windows.");
            return 2;
        }

        var processRunner = new ProcessRunner();
        var metadataProvider = new PowerShellDriverMetadataProvider(processRunner);
        var drivers = await metadataProvider.GetInstalledDriversAsync();
        var exactInf = drivers
            .Select(driver => driver.InfName?.Trim())
            .FirstOrDefault(name =>
                !string.IsNullOrWhiteSpace(name) &&
                Regex.IsMatch(
                    name,
                    "^oem[0-9]+\\.inf$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));

        if (exactInf is null)
        {
            Console.Error.WriteLine("DriverFix backup export smoke found no exact published oem#.inf in installed-driver metadata.");
            return 1;
        }

        var targetDirectory = Path.Combine(
            Path.GetTempPath(),
            $"DriverFix-backup-smoke-{Guid.NewGuid():N}");

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var backupService = new PnpUtilDriverBackupService(processRunner);
            var result = await backupService.ExportExactInfAsync(
                exactInf,
                targetDirectory,
                timeout.Token);

            if (!result.IsVerified)
            {
                Console.Error.WriteLine($"DriverFix backup export smoke blocked: {result.Evidence}");
                return 1;
            }

            Console.WriteLine("Backup export smoke: PASS");
            Console.WriteLine($"Published INF: {result.InfName}");
            Console.WriteLine($"Exported files: {result.ExportedFiles.Count}");
            Console.WriteLine($"Exported bytes: {result.TotalBytes}");
            Console.WriteLine(result.Evidence);
            return 0;
        }
        finally
        {
            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, recursive: true);
        }
    }

    private static async Task<int> RunElevationSmokeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix elevation smoke requires Windows.");
            return 2;
        }

        var workerPath = Path.Combine(AppContext.BaseDirectory, "DriverFix.Elevated.exe");
        var broker = new ElevatedWorkerBroker();
        var response = await broker.ExecuteAsync(workerPath, ElevatedOperation.Probe);

        if (!response.Accepted)
        {
            Console.Error.WriteLine($"DriverFix elevation smoke failed: {response.Evidence}");
            return 1;
        }

        Console.WriteLine(response.Evidence);
        return 0;
    }
}
