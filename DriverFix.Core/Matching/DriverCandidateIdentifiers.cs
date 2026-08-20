namespace DriverFix.Core.Matching;

public sealed record DriverCandidateIdentifiers(
    IReadOnlyList<string> HardwareIds,
    IReadOnlyList<string> CompatibleIds
);
