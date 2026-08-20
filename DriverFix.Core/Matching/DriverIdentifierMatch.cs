namespace DriverFix.Core.Matching;

public sealed record DriverIdentifierMatch(
    DriverIdentifierMatchKind Kind,
    int Score,
    string? DeviceIdentifier,
    string? CandidateIdentifier
)
{
    public bool IsMatch => Kind != DriverIdentifierMatchKind.None && Score > 0;
}
