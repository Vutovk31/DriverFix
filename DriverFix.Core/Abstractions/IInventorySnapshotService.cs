using DriverFix.Core.Models;

namespace DriverFix.Core.Abstractions;

public interface IInventorySnapshotService
{
    Task<InventoryCaptureResult> CaptureAsync(
        CancellationToken cancellationToken = default);
}
