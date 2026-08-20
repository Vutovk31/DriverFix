namespace DriverFix.Core.Models;

public sealed record DeviceSnapshot(
    DeviceInventoryItem Device,
    DriverMetadata? InstalledDriver
);
