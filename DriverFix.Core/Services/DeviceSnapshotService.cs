using DriverFix.Core.Abstractions;
using DriverFix.Core.Models;

namespace DriverFix.Core.Services;

public sealed class DeviceSnapshotService
{
    private readonly IDriverMetadataProvider _driverMetadataProvider;

    public DeviceSnapshotService(IDriverMetadataProvider driverMetadataProvider)
    {
        _driverMetadataProvider = driverMetadataProvider ??
            throw new ArgumentNullException(nameof(driverMetadataProvider));
    }

    public async Task<IReadOnlyList<DeviceSnapshot>> JoinAsync(
        InventorySnapshot inventory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = await _driverMetadataProvider.GetInstalledDriversAsync(cancellationToken);
        var byDeviceId = metadata
            .GroupBy(item => Normalize(item.DeviceId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return inventory.Devices
            .Select(device => new DeviceSnapshot(
                device,
                byDeviceId.TryGetValue(Normalize(device.InstanceId), out var driver)
                    ? driver
                    : null))
            .ToArray();
    }

    private static string Normalize(string value) =>
        value.Trim().Replace('/', '\\');
}
