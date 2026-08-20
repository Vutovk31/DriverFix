using DriverFix.Core.Models;

namespace DriverFix.Core.Abstractions;

public interface IDeviceInventoryProvider
{
    Task<IReadOnlyList<DeviceInventoryItem>> GetConnectedDevicesAsync(
        CancellationToken cancellationToken = default);
}
