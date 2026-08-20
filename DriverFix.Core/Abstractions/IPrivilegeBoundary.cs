using DriverFix.Core.Security;

namespace DriverFix.Core.Abstractions;

public interface IPrivilegeBoundary
{
    PrivilegeCheckResult CheckCurrentProcess();
}
