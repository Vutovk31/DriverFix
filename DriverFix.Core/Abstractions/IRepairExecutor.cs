using DriverFix.Core.Models;
using DriverFix.Core.Repair;

namespace DriverFix.Core.Abstractions;

public interface IDeviceSnapshotReader
{
    Task<DeviceSnapshot?> ReadAsync(string instanceId, CancellationToken cancellationToken = default);
}

public interface IRepairExecutor
{
    Task<RepairResult> ExecuteAsync(RepairRequest request, CancellationToken cancellationToken = default);
}
