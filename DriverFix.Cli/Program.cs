using DriverFix.Core.Failures;
using DriverFix.Core.Services;
using DriverFix.Windows;

namespace DriverFix.Cli;

internal static class Program
{
    public static async Task<int> Main()
    {
        try
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
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("DriverFix inventory cancelled.");
            return 3;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DriverFix inventory failed: {ex.Message}");
            return 1;
        }
    }
}
