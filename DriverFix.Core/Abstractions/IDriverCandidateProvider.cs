using DriverFix.Core.Candidates;

namespace DriverFix.Core.Abstractions;

public interface IDriverCandidateProvider
{
    Task<IReadOnlyList<DriverUpdateCandidate>> SearchAsync(
        CancellationToken cancellationToken = default);
}
