using DriverFix.Core.Failures;
using DriverFix.Windows;

namespace DriverFix.Cli;

internal static class Program
{
    public static async Task<int> Main()
    {
        try
        {
            var provider = new PnpUtilDeviceInventoryProvider(new ProcessRunner());
            var devices = await provider.GetConnectedDevicesAsync();

            Console.WriteLine(DeviceInventoryTextFormatter.Format(devices));
            return 0;
        }
        catch (InventoryProviderException ex)
        {
            Console.Error.WriteLine(
                $"DriverFix inventory failed [{ex.Kind}]: {ex.Message}");

            return ex.Kind == InventoryFailureKind.PlatformUnsupported
                ? 2
                : 1;
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
