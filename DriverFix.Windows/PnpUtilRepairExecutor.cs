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

        var preflight = RepairPreflightService.Evaluate(request);
        if (!preflight.IsAllowed)
            return Blocked(preflight.Evidence);

        var fullInf = preflight.FullInfPath!;

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
