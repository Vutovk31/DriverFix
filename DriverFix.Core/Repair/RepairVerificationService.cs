using DriverFix.Core.Models;

namespace DriverFix.Core.Repair;

public static class RepairVerificationService
{
    public static bool IsVerified(DeviceSnapshot before, DeviceSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);

        if (!string.Equals(before.Device.InstanceId, after.Device.InstanceId, StringComparison.OrdinalIgnoreCase))
            return false;

        if (after.Device.ProblemCode is > 0)
            return false;

        var beforeDriver = before.InstalledDriver;
        var afterDriver = after.InstalledDriver;
        var identityChanged = !string.Equals(beforeDriver?.InfName, afterDriver?.InfName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(beforeDriver?.DriverVersion, afterDriver?.DriverVersion, StringComparison.OrdinalIgnoreCase);
        var problemCleared = before.Device.ProblemCode is > 0 && (after.Device.ProblemCode is null or 0);

        return identityChanged || problemCleared;
    }
}
