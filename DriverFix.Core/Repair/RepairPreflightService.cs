namespace DriverFix.Core.Repair;

public sealed record RepairPreflightResult(
    bool IsAllowed,
    string Evidence,
    string? FullInfPath = null
);

public static class RepairPreflightService
{
    public static RepairPreflightResult Evaluate(RepairRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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
        if (string.IsNullOrWhiteSpace(request.CandidateInfPath) ||
            Path.GetExtension(request.CandidateInfPath) is not string ext ||
            !ext.Equals(".inf", StringComparison.OrdinalIgnoreCase))
            return Blocked("Candidate must be one exact INF file.");
        if (request.CandidateInfPath.IndexOf('*') >= 0 || request.CandidateInfPath.IndexOf('?') >= 0)
            return Blocked("Wildcards are not allowed for repair INF.");

        var fullInf = Path.GetFullPath(request.CandidateInfPath);
        if (!File.Exists(fullInf))
            return Blocked("Candidate INF does not exist.");

        return new(true, "Repair preflight passed.", fullInf);
    }

    private static RepairPreflightResult Blocked(string evidence) => new(false, evidence);
}
