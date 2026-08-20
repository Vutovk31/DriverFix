namespace DriverFix.Core.Models;

public sealed record InventorySnapshot(
    DateTimeOffset CapturedAtUtc,
    IReadOnlyList<DeviceInventoryItem> Devices
);
