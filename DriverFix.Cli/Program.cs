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
        catch (PlatformNotSupportedException ex)
        {
            Console.Error.WriteLine($"DriverFix inventory is supported on Windows only: {ex.Message}");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DriverFix inventory failed: {ex.Message}");
            return 1;
        }
    }
}
