using System.Text.Json;
using DriverFix.Core.Candidates;

namespace DriverFix.Windows;

public static class DriverCandidateJsonParser
{
    public static IReadOnlyList<DriverUpdateCandidate> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<DriverUpdateCandidate>();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(ParseItem).ToArray();

        if (root.ValueKind == JsonValueKind.Object)
            return new[] { ParseItem(root) };

        return Array.Empty<DriverUpdateCandidate>();
    }

    private static DriverUpdateCandidate ParseItem(JsonElement item) => new(
        GetString(item, "UpdateId") ?? string.Empty,
        GetString(item, "Title") ?? string.Empty,
        GetString(item, "DriverProvider"),
        GetString(item, "DriverManufacturer"),
        GetString(item, "DriverModel"),
        GetString(item, "DriverClass"),
        GetString(item, "DriverVerDate"),
        GetString(item, "DriverHardwareID"),
        GetBool(item, "IsDownloaded"),
        GetBool(item, "IsHidden"),
        GetBool(item, "EulaAccepted"));

    private static string? GetString(JsonElement item, string name) =>
        item.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : null;

    private static bool GetBool(JsonElement item, string name) =>
        item.TryGetProperty(name, out var value) && value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            _ => false
        };
}
