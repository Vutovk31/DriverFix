using DriverFix.Core.Models;

namespace DriverFix.Core.Matching;

public static class DriverIdentifierMatcher
{
    public static DriverIdentifierMatch Match(
        DeviceInventoryItem device,
        DriverCandidateIdentifiers candidate)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(candidate);

        return FirstExact(device.HardwareIds, candidate.HardwareIds,
                   DriverIdentifierMatchKind.HardwareToHardware, 4000)
            ?? FirstExact(device.HardwareIds, candidate.CompatibleIds,
                   DriverIdentifierMatchKind.HardwareToCompatible, 3000)
            ?? FirstExact(device.CompatibleIds, candidate.HardwareIds,
                   DriverIdentifierMatchKind.CompatibleToHardware, 2000)
            ?? FirstExact(device.CompatibleIds, candidate.CompatibleIds,
                   DriverIdentifierMatchKind.CompatibleToCompatible, 1000)
            ?? new DriverIdentifierMatch(
                DriverIdentifierMatchKind.None,
                0,
                null,
                null);
    }

    private static DriverIdentifierMatch? FirstExact(
        IReadOnlyList<string> deviceIds,
        IReadOnlyList<string> candidateIds,
        DriverIdentifierMatchKind kind,
        int baseScore)
    {
        for (var deviceIndex = 0; deviceIndex < deviceIds.Count; deviceIndex++)
        {
            var deviceId = Normalize(deviceIds[deviceIndex]);
            if (deviceId.Length == 0)
                continue;

            for (var candidateIndex = 0; candidateIndex < candidateIds.Count; candidateIndex++)
            {
                var candidateId = Normalize(candidateIds[candidateIndex]);
                if (candidateId.Length == 0)
                    continue;

                if (!string.Equals(deviceId, candidateId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var positionPenalty = deviceIndex + candidateIndex;
                return new DriverIdentifierMatch(
                    kind,
                    Math.Max(1, baseScore - positionPenalty),
                    deviceIds[deviceIndex].Trim(),
                    candidateIds[candidateIndex].Trim());
            }
        }

        return null;
    }

    private static string Normalize(string value) =>
        value.Trim();
}
