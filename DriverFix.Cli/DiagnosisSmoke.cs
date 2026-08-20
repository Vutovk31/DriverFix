using DriverFix.Core.Diagnosis;
using DriverFix.Core.Services;
using DriverFix.Windows;

namespace DriverFix.Cli;

internal static class DiagnosisSmoke
{
    public static async Task<int> RunAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("DriverFix diagnosis smoke requires Windows.");
            return 2;
        }

        var processRunner = new ProcessRunner();
        var inventoryProvider = new PnpUtilDeviceInventoryProvider(processRunner);
        var inventoryService = new InventorySnapshotService(inventoryProvider);
        var inventory = await inventoryService.CaptureAsync();

        if (!inventory.Succeeded)
        {
            var failure = inventory.Failure;
            Console.Error.WriteLine(
                $"DriverFix diagnosis smoke inventory failed [{failure.Kind}]: {failure.Message}");
            return 1;
        }

        if (inventory.Snapshot.Devices.Count == 0)
        {
            Console.Error.WriteLine("DriverFix diagnosis smoke found no connected devices.");
            return 1;
        }

        var metadataProvider = new PowerShellDriverMetadataProvider(processRunner);
        var snapshotService = new DeviceSnapshotService(metadataProvider);
        var snapshots = await snapshotService.JoinAsync(inventory.Snapshot);
        var diagnoses = snapshots.Select(DiagnosisEngine.Diagnose).ToArray();

        if (diagnoses.Length != inventory.Snapshot.Devices.Count)
        {
            Console.Error.WriteLine(
                $"DriverFix diagnosis smoke count mismatch: devices={inventory.Snapshot.Devices.Count}, diagnoses={diagnoses.Length}.");
            return 1;
        }

        Console.WriteLine("Diagnosis smoke: PASS");
        Console.WriteLine($"Connected devices: {inventory.Snapshot.Devices.Count}");
        Console.WriteLine($"Diagnoses: {diagnoses.Length}");

        foreach (var group in diagnoses
                     .GroupBy(diagnosis => diagnosis.Kind)
                     .OrderBy(group => group.Key.ToString(), StringComparer.Ordinal))
        {
            Console.WriteLine($"Diagnosis {group.Key}: {group.Count()}");
        }

        return 0;
    }
}
