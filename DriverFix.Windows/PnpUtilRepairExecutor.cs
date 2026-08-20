using DriverFix.Core.Abstractions;
using DriverFix.Core.Repair;

namespace DriverFix.Windows;

public sealed class PnpUtilRepairExecutor : IRepairExecutor
{
    private readonly IProcessRunner _processRunner;
    private readonly IDeviceSnapshotReader _snapshotReader;

    public PnpUtilRepairExecutor(IProcessRunner processRunner, IDeviceSnapshotReader snapshotReader)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
    }

    public async Task<RepairResult> ExecuteAsync(RepairRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.TransactionId))
            return Blocked("TransactionId is required.");
        if (string.IsNullOrWhiteSpace(request.TargetInstanceId))
            return Blocked("Target InstanceId is required.");
        if (!request.CompatibilityVerified || request.DriverFixScore <= 0)
            return Blocked("Compatibility is not verified.");
        if (!request.Backup.IsVerified || request.Backup.TotalBytes <= 0)
            return Blocked("Verified driver backup is required before repair.");
        if (request.ConnectedMatchingDeviceCount != 1)
            return Blocked("Repair requires exactly one connected matching device because PnPUtil /install can update any matching devices.");
        if (!string.Equals(request.BeforeSnapshot.Device.InstanceId, request.TargetInstanceId, StringComparison.OrdinalIgnoreCase))
            return Blocked("Before snapshot does not belong to target device.");
        if (string.IsNullOrWhiteSpace(request.CandidateInfPath) || Path.GetExtension(request.CandidateInfPath) is not string ext || !ext.Equals(".inf", StringComparison.OrdinalIgnoreCase))
            return Blocked("Candidate must be one exact INF file.");
        if (request.CandidateInfPath.IndexOf('*') >= 0 || request.CandidateInfPath.IndexOf('?') >= 0)
            return Blocked("Wildcards are not allowed for repair INF.");

        var fullInf = Path.GetFullPath(request.CandidateInfPath);
        if (!File.Exists(fullInf))
            return Blocked("Candidate INF does not exist.");

        ProcessResult install;
        try
        {
            install = await _processRunner.RunAsync(
                "pnputil.exe",
                new[] { "/add-driver", fullInf, "/install" },
                cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new(RepairOutcome.ManualRecoveryRequired,
                $"Install mutation state is unknown after process failure: {ex.Message}");
        }

        if (install.ExitCode is 3010 or 1641)
            return new(RepairOutcome.AwaitingSystemReboot,
                $"Driver installation requires system reboot (exit {install.ExitCode}).", RebootRequired: true);
        if (install.ExitCode == 259)
            return new(RepairOutcome.InstallRejected, "PnPUtil rejected the install request (exit 259).");
        if (install.ExitCode != 0)
            return new(RepairOutcome.InstallRejected, $"PnPUtil install failed with exit code {install.ExitCode}.");

        var restart = await _processRunner.RunAsync(
            "pnputil.exe",
            new[] { "/restart-device", request.TargetInstanceId },
            cancellationToken);
        if (restart.ExitCode != 0)
            return new(RepairOutcome.RollbackRequired,
                $"Driver install succeeded but targeted device restart failed with exit code {restart.ExitCode}.");

        var after = await _snapshotReader.ReadAsync(request.TargetInstanceId, cancellationToken);
        if (after is null)
            return new(RepairOutcome.RollbackRequired, "Post-repair device snapshot is unavailable.");

        return RepairVerificationService.IsVerified(request.BeforeSnapshot, after)
            ? new(RepairOutcome.Verified, "Repair verified by healthy target state plus meaningful driver/problem-state change.", after)
            : new(RepairOutcome.RollbackRequired, "Repair mutation completed but the original problem was not proven fixed.", after);
    }

    private static RepairResult Blocked(string evidence) => new(RepairOutcome.Blocked, evidence);
}
