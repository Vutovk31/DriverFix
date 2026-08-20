using DriverFix.Core.Matching;

namespace DriverFix.Core.Candidates;

public sealed record DriverUpdateCandidate(
    string UpdateId,
    string Title,
    string? DriverProvider,
    string? DriverManufacturer,
    string? DriverModel,
    string? DriverClass,
    string? DriverVerDate,
    string? SourceMatchIdentifier,
    bool IsDownloaded,
    bool IsHidden,
    bool EulaAccepted)
{
    public DriverCandidateIdentifiers ToIdentifiers() =>
        string.IsNullOrWhiteSpace(SourceMatchIdentifier)
            ? new(Array.Empty<string>(), Array.Empty<string>())
            : new(Array.Empty<string>(), new[] { SourceMatchIdentifier.Trim() });
}
