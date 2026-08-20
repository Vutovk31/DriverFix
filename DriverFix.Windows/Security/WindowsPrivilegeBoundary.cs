using System.Security.Principal;
using DriverFix.Core.Abstractions;
using DriverFix.Core.Security;

namespace DriverFix.Windows.Security;

public sealed class WindowsPrivilegeBoundary : IPrivilegeBoundary
{
    public PrivilegeCheckResult CheckCurrentProcess()
    {
        if (!OperatingSystem.IsWindows())
            return new(false, false, "Privilege check is only meaningful on Windows.");

        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var elevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

        return new(true, elevated,
            elevated
                ? "Current process token is in the Administrators role."
                : "Current process is not elevated; privileged mutation must use an explicit elevated boundary.");
    }
}
