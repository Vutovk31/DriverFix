namespace DriverFix.Core.Models;

public sealed record DeviceInventoryItem(
    string InstanceId,
    string? DeviceDescription,
    string? ClassName,
    string? Manufacturer,
    string? Status,
    int? ProblemCode,
    IReadOnlyList<string> HardwareIds,
    IReadOnlyList<string> CompatibleIds
);
