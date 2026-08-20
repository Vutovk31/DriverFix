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

            if (args.Length == 1 && string.Equals(args[0], "--wua-candidate-smoke", StringComparison.Ordinal))
                return await RunWindowsUpdateCandidateSmokeAsync();

            if (args.Length != 0)
            {
                Console.Error.WriteLine("Usage: DriverFix.exe [--elevation-smoke|--driver-metadata-smoke|--wua-candidate-smoke]");
                return 64;
            }

            return await RunInventoryAsync();
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
