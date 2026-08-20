using DriverFix.Core.Models;
using DriverFix.Core.Rollback;

namespace DriverFix.Core.Abstractions;

public interface IRollbackExecutor
{
    Task<RollbackResult> ExecuteAsync(RollbackRequest request, CancellationToken cancellationToken = default);
}
