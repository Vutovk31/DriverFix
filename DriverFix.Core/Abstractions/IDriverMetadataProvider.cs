using DriverFix.Core.Models;

namespace DriverFix.Core.Abstractions;

public interface IDriverMetadataProvider
{
    Task<IReadOnlyList<DriverMetadata>> GetInstalledDriversAsync(
        CancellationToken cancellationToken = default);
}
