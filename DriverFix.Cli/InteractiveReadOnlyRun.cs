using DriverFix.Core.Abstractions;
using DriverFix.Core.Diagnosis;
using DriverFix.Core.Failures;
using DriverFix.Core.Models;
using DriverFix.Core.Services;
using DriverFix.Windows;

namespace DriverFix.Cli;

internal static class InteractiveReadOnlyRun
{
    public static async Task<int> RunAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix read-only diagnosis requires Windows.");
            return 2;
        }

        Console.WriteLine("DriverFix read-only diagnosis");
        Console.WriteLine("No drivers or devices will be changed.");
        Console.WriteLine();

        var processRunner = new ProcessRunner();

        Console.WriteLine("Scanning connected devices...");
        var inventoryProvider = new PnpUtilDeviceInventoryProvider(processRunner);
        var inventoryService = new InventorySnapshotService(inventoryProvider);
        var inventory = await inventoryService.CaptureAsync();

        if (!inventory.Succeeded)
        {
            var failure = inventory.Failure;
            Console.Error.WriteLine(
                $"DriverFix inventory failed [{failure.Kind}]: {failure.Message}");
            return failure.Kind == InventoryFailureKind.PlatformUnsupported ? 2 : 1;
        }

        Console.WriteLine($"Connected devices: {inventory.Snapshot.Devices.Count}");

        Console.WriteLine("Reading installed driver metadata...");
        var metadataProvider = new PowerShellDriverMetadataProvider(processRunner);
        var metadata = await metadataProvider.GetInstalledDriversAsync();
        Console.WriteLine($"Installed driver metadata: {metadata.Count}");

        if (metadata.Count == 0)
        {
            Console.Error.WriteLine("DriverFix returned no installed driver metadata records.");
            return 1;
        }

        Console.WriteLine("Analyzing driver state...");
        var snapshotService = new DeviceSnapshotService(new PreloadedDriverMetadataProvider(metadata));
        var snapshots = await snapshotService.JoinAsync(inventory.Snapshot);
        var diagnoses = snapshots.Select(DiagnosisEngine.Diagnose).ToArray();

        var problemCount = diagnoses.Count(diagnosis => diagnosis.Kind != DiagnosisKind.Healthy);

        Console.WriteLine($"Diagnoses: {diagnoses.Length}");
        Console.WriteLine($"Problems found: {problemCount}");

        foreach (var group in diagnoses
                     .GroupBy(diagnosis => diagnosis.Kind)
                     .OrderBy(group => group.Key.ToString(), StringComparer.Ordinal))
        {
            Console.WriteLine($"Diagnosis {group.Key}: {group.Count()}");
        }

        Console.WriteLine();
        Console.WriteLine("Read-only diagnosis completed successfully.");
        return 0;
    }

    public static bool ShouldPauseForInteractiveUser()
    {
        if (!Environment.UserInteractive || Console.IsInputRedirected || Console.IsOutputRedirected)
            return false;

        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI"));
    }

    public static void PauseForInteractiveUser()
    {
        if (!ShouldPauseForInteractiveUser())
            return;

        Console.WriteLine();
        Console.Write("Press any key to exit...");
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }

    private sealed class PreloadedDriverMetadataProvider : IDriverMetadataProvider
    {
        private readonly IReadOnlyList<DriverMetadata> _metadata;

        public PreloadedDriverMetadataProvider(IReadOnlyList<DriverMetadata> metadata)
        {
            _metadata = metadata;
        }

        public Task<IReadOnlyList<DriverMetadata>> GetInstalledDriversAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_metadata);
        }
    }
}
