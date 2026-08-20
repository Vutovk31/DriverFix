using DriverFix.Core.Abstractions;
using DriverFix.Core.Failures;
using DriverFix.Core.Models;

namespace DriverFix.Core.Services;

public sealed class InventorySnapshotService : IInventorySnapshotService
{
    private readonly IDeviceInventoryProvider _provider;
    private readonly TimeProvider _timeProvider;

    public InventorySnapshotService(
        IDeviceInventoryProvider provider,
        TimeProvider? timeProvider = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<InventoryCaptureResult> CaptureAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var devices = await _provider.GetConnectedDevicesAsync(cancellationToken);
            var snapshot = new InventorySnapshot(
                _timeProvider.GetUtcNow(),
                devices.ToArray());

            return InventoryCaptureResult.Success(snapshot);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InventoryProviderException ex)
        {
            return InventoryCaptureResult.Failed(
                new InventoryFailureEvidence(
                    ex.Kind,
                    ex.Message,
                    ex.ExitCode,
                    ex.StandardError));
        }
        catch (Exception ex)
        {
            return InventoryCaptureResult.Failed(
                new InventoryFailureEvidence(
                    InventoryFailureKind.UnexpectedFailure,
                    ex.Message,
                    null,
                    null));
        }
    }
}
