using System.Text.Json;
using DriverFix.Core.Models;

namespace DriverFix.Windows;

public static class DriverMetadataJsonParser
{
    public static IReadOnlyList<DriverMetadata> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<DriverMetadata>();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(ParseItem).ToArray();

        if (root.ValueKind == JsonValueKind.Object)
            return new[] { ParseItem(root) };

        return Array.Empty<DriverMetadata>();
    }

    private static DriverMetadata ParseItem(JsonElement item) =>
        new(
            GetString(item, "DeviceID") ?? string.Empty,
            GetString(item, "DeviceName"),
            GetString(item, "DriverProviderName"),
            GetString(item, "DriverVersion"),
            GetString(item, "DriverDate"),
            GetString(item, "InfName"),
            GetBoolean(item, "IsSigned"),
            GetString(item, "Signer"));

    private static string? GetString(JsonElement item, string property) =>
        item.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : null;

    private static bool? GetBoolean(JsonElement item, string property)
    {
        if (!item.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            _ => null
        };
    }
}
