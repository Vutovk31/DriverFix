using DriverFix.Core.Models;

namespace DriverFix.Core.Rollback;

public static class RollbackVerificationService
{
    public static bool IsVerified(DeviceSnapshot original, DeviceSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(after);

        if (!string.Equals(original.Device.InstanceId, after.Device.InstanceId, StringComparison.OrdinalIgnoreCase))
            return false;
        if (after.Device.ProblemCode is > 0)
            return false;

        var originalDriver = original.InstalledDriver;
        var afterDriver = after.InstalledDriver;
        var infRestored = !string.IsNullOrWhiteSpace(originalDriver?.InfName)
            && string.Equals(originalDriver!.InfName, afterDriver?.InfName, StringComparison.OrdinalIgnoreCase);
        var versionRestored = !string.IsNullOrWhiteSpace(originalDriver?.DriverVersion)
            && string.Equals(originalDriver!.DriverVersion, afterDriver?.DriverVersion, StringComparison.OrdinalIgnoreCase);

        return infRestored || versionRestored;
    }
}
