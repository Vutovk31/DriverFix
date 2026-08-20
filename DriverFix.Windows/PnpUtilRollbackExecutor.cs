using DriverFix.Core.Abstractions;
using DriverFix.Core.Rollback;

namespace DriverFix.Windows;

public sealed class PnpUtilRollbackExecutor : IRollbackExecutor
{
    private readonly IProcessRunner _processRunner;
    private readonly IDeviceSnapshotReader _snapshotReader;

    public PnpUtilRollbackExecutor(IProcessRunner processRunner, IDeviceSnapshotReader snapshotReader)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
    }

    public async Task<RollbackResult> ExecuteAsync(RollbackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.TransactionId))
            return Blocked("TransactionId is required.");
        if (string.IsNullOrWhiteSpace(request.TargetInstanceId))
            return Blocked("Target InstanceId is required.");
        if (!request.Backup.IsVerified || request.Backup.TotalBytes <= 0)
            return Blocked("Verified backup is required before rollback.");
        if (request.ConnectedMatchingDeviceCount != 1)
            return Blocked("Rollback requires exactly one connected matching device because PnPUtil /install can update any matching devices.");
        if (!string.Equals(request.OriginalSnapshot.Device.InstanceId, request.TargetInstanceId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(request.FailedSnapshot.Device.InstanceId, request.TargetInstanceId, StringComparison.OrdinalIgnoreCase))
            return Blocked("Rollback snapshots must belong to the target device.");
        if (string.IsNullOrWhiteSpace(request.BackupInfPath)
            || !string.Equals(Path.GetExtension(request.BackupInfPath), ".inf", StringComparison.OrdinalIgnoreCase)
            || request.BackupInfPath.IndexOf('*') >= 0
            || request.BackupInfPath.IndexOf('?') >= 0)
            return Blocked("Rollback requires one exact backup INF file.");

        var fullInf = Path.GetFullPath(request.BackupInfPath);
        if (!File.Exists(fullInf))
            return Blocked("Backup INF does not exist.");

        ProcessResult restore;
        try
        {
            restore = await _processRunner.RunAsync(
                "pnputil.exe",
                new[] { "/add-driver", fullInf, "/install" },
                cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new(RollbackOutcome.ManualRecoveryRequired,
                $"Rollback mutation state is unknown after process failure: {ex.Message}");
        }

        if (restore.ExitCode is 3010 or 1641)
            return new(RollbackOutcome.AwaitingSystemReboot,
                $"Rollback restore requires system reboot (exit {restore.ExitCode}).", RebootRequired: true);
        if (restore.ExitCode == 259)
            return new(RollbackOutcome.ManualRecoveryRequired,
                "PnPUtil rejected rollback restore (exit 259); no force/delete fallback is allowed.");
        if (restore.ExitCode != 0)
            return new(RollbackOutcome.RestoreRejected,
                $"PnPUtil rollback restore failed with exit code {restore.ExitCode}.");

        var restart = await _processRunner.RunAsync(
            "pnputil.exe",
            new[] { "/restart-device", request.TargetInstanceId },
            cancellationToken);
        if (restart.ExitCode != 0)
            return new(RollbackOutcome.ManualRecoveryRequired,
                $"Rollback restore succeeded but targeted restart failed with exit code {restart.ExitCode}.");

        var after = await _snapshotReader.ReadAsync(request.TargetInstanceId, cancellationToken);
        if (after is null)
            return new(RollbackOutcome.ManualRecoveryRequired,
                "Post-rollback snapshot is unavailable.");

        return RollbackVerificationService.IsVerified(request.OriginalSnapshot, after)
            ? new(RollbackOutcome.RolledBack, "Rollback verified by healthy target state plus restored original INF or version.", after)
            : new(RollbackOutcome.ManualRecoveryRequired, "Rollback mutation completed but restoration could not be proven.", after);
    }

    private static RollbackResult Blocked(string evidence) => new(RollbackOutcome.Blocked, evidence);
}
