using DriverFix.Core.Failures;

namespace DriverFix.Core.Models;

public sealed class InventoryCaptureResult
{
    private readonly InventorySnapshot? _snapshot;
    private readonly InventoryFailureEvidence? _failure;

    private InventoryCaptureResult(
        InventorySnapshot? snapshot,
        InventoryFailureEvidence? failure)
    {
        _snapshot = snapshot;
        _failure = failure;
    }

    public bool Succeeded => _snapshot is not null;

    public InventorySnapshot Snapshot => _snapshot ??
        throw new InvalidOperationException("Capture result does not contain a snapshot.");

    public InventoryFailureEvidence Failure => _failure ??
        throw new InvalidOperationException("Capture result does not contain failure evidence.");

    public static InventoryCaptureResult Success(InventorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new InventoryCaptureResult(snapshot, null);
    }

    public static InventoryCaptureResult Failed(InventoryFailureEvidence failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new InventoryCaptureResult(null, failure);
    }
}
