using DriverFix.Core.Candidates;
using DriverFix.Core.Matching;
using DriverFix.Core.Models;

namespace DriverFix.Core.Services;

public sealed record CandidateEligibility(
    DriverUpdateCandidate Candidate,
    DriverIdentifierMatch Match,
    bool IsEligible,
    string Reason
);

public static class DriverCandidateEligibilityEvaluator
{
    public static CandidateEligibility Evaluate(
        DeviceInventoryItem device,
        DriverUpdateCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(candidate);

        var match = DriverIdentifierMatcher.Match(device, candidate.ToIdentifiers());

        if (!match.IsMatch)
            return new(candidate, match, false,
                "Candidate has no exact hardware/compatible identifier match.");

        if (!candidate.EulaAccepted)
            return new(candidate, match, false,
                "Candidate EULA is not accepted; DriverFix will not accept or install it implicitly.");

        if (candidate.IsHidden)
            return new(candidate, match, false,
                "Candidate is hidden and is not eligible for repair selection.");

        return new(candidate, match, true,
            "Candidate has exact identifier evidence and no discovery-stage policy block.");
    }
}
