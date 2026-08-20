using DriverFix.Core.Elevation;

namespace DriverFix.Windows.Elevation;

public static class ElevatedOperationValidator
{
    public static string? Validate(ElevatedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nonce))
            return "Nonce is required.";

        return request.Operation switch
        {
            ElevatedOperation.InstallExactInf => ValidateExactInf(request.InfPath),
            ElevatedOperation.RestartExactDevice =>
                string.IsNullOrWhiteSpace(request.InstanceId) ? "InstanceId is required." : null,
            ElevatedOperation.RestoreExactBackup => ValidateExactInf(request.InfPath),
            ElevatedOperation.Probe =>
                request.InfPath is not null || request.InstanceId is not null
                    ? "Probe does not accept mutation arguments."
                    : null,
            _ => "Operation is not allowed."
        };
    }

    private static string? ValidateExactInf(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "Exact INF path is required.";
        if (path.IndexOf('*') >= 0 || path.IndexOf('?') >= 0)
            return "Wildcards are not allowed.";
        if (!string.Equals(Path.GetExtension(path), ".inf", StringComparison.OrdinalIgnoreCase))
            return "Only one exact .inf file is allowed.";

        var full = Path.GetFullPath(path);
        return File.Exists(full) ? null : "INF file does not exist.";
    }
}
