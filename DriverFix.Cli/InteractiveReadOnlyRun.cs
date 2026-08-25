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
        try
        {
            return await ExecuteAsync();
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
        finally
        {
            PauseForInteractiveUser();
        }
    }

    private static async Task<int> ExecuteAsync()
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
            Console.Error.WriteLine($"DriverFix inventory failed [{failure.Kind}]: {failure.Message}");
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
        var results = snapshots
            .Select(snapshot => (Snapshot: snapshot, Diagnosis: DiagnosisEngine.Diagnose(snapshot)))
            .ToArray();
        var diagnoses = results.Select(result => result.Diagnosis).ToArray();
        var problems = results
            .Where(result => result.Diagnosis.Kind != DiagnosisKind.Healthy)
            .ToArray();

        Console.WriteLine($"Diagnoses: {diagnoses.Length}");
        Console.WriteLine($"Problems found: {problems.Length}");

        foreach (var group in diagnoses.GroupBy(d => d.Kind).OrderBy(g => g.Key.ToString(), StringComparer.Ordinal))
            Console.WriteLine($"Diagnosis {group.Key}: {group.Count()}");

        if (problems.Length > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Problem devices:");

            foreach (var result in problems)
            {
                var device = result.Snapshot.Device;
                var description = string.IsNullOrWhiteSpace(device.DeviceDescription)
                    ? "(description unavailable)"
                    : device.DeviceDescription.Trim();

                Console.WriteLine($"- {description}");
                Console.WriteLine($"  Instance ID: {device.InstanceId}");
                Console.WriteLine($"  Diagnosis: {result.Diagnosis.Kind} ({result.Diagnosis.Confidence})");
                Console.WriteLine($"  Evidence: {result.Diagnosis.Evidence}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Read-only diagnosis completed successfully.");
        return 0;
    }

    private static void PauseForInteractiveUser()
    {
        if (!Environment.UserInteractive || Console.IsInputRedirected || Console.IsOutputRedirected)
            return;
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI")))
            return;

        Console.WriteLine();
        Console.Write("Press any key to exit...");
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }

    private sealed class PreloadedDriverMetadataProvider : IDriverMetadataProvider
    {
        private readonly IReadOnlyList<DriverMetadata> _metadata;
        public PreloadedDriverMetadataProvider(IReadOnlyList<DriverMetadata> metadata) => _metadata = metadata;
        public Task<IReadOnlyList<DriverMetadata>> GetInstalledDriversAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_metadata);
        }
    }
}
