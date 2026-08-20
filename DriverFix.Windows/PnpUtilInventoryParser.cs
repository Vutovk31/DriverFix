using System.Text.RegularExpressions;
using DriverFix.Core.Models;

namespace DriverFix.Windows;

public static class PnpUtilInventoryParser
{
    private static readonly string[] InstanceIdKeys =
    {
        "Instance ID",
        "Идентификатор экземпляра"
    };

    public static IReadOnlyList<DeviceInventoryItem> Parse(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return Array.Empty<DeviceInventoryItem>();

        var devices = new List<DeviceInventoryItem>();

        foreach (var block in SplitDeviceBlocks(output))
        {
            var fields = ParseFields(block);
            var instanceId = First(fields, "Instance ID", "Идентификатор экземпляра");

            if (string.IsNullOrWhiteSpace(instanceId))
                continue;

            devices.Add(new DeviceInventoryItem(
                instanceId,
                First(fields, "Device Description", "Описание устройства"),
                First(fields, "Class Name", "Имя класса"),
                First(fields, "Manufacturer Name", "Имя изготовителя", "Производитель"),
                First(fields, "Status", "Состояние"),
                ParseProblemCode(First(fields, "Problem Code", "Код проблемы")),
                Values(fields, "Hardware IDs", "ИД оборудования"),
                Values(fields, "Compatible IDs", "Совместимые ИД")));
        }

        return devices;
    }

    private static IReadOnlyList<IReadOnlyList<string>> SplitDeviceBlocks(string output)
    {
        var blocks = new List<IReadOnlyList<string>>();
        var current = new List<string>();
        var seenInstance = false;

        foreach (var raw in output.Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = raw.Trim();
            var isInstance = InstanceIdKeys.Any(key =>
                trimmed.StartsWith(key + ":", StringComparison.OrdinalIgnoreCase));

            if (isInstance && seenInstance && current.Count > 0)
            {
                blocks.Add(current.ToArray());
                current.Clear();
            }

            if (isInstance)
                seenInstance = true;

            if (seenInstance)
                current.Add(raw);
        }

        if (current.Count > 0)
            blocks.Add(current.ToArray());

        return blocks;
    }

    private static Dictionary<string, List<string>> ParseFields(
        IReadOnlyList<string> lines)
    {
        var result = new Dictionary<string, List<string>>(
            StringComparer.OrdinalIgnoreCase);
        string? currentKey = null;

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var colon = raw.IndexOf(':');
            if (colon > 0)
            {
                currentKey = raw[..colon].Trim();
                var value = raw[(colon + 1)..].Trim();

                if (!result.TryGetValue(currentKey, out var list))
                {
                    list = new List<string>();
                    result[currentKey] = list;
                }

                if (!string.IsNullOrWhiteSpace(value))
                    list.Add(value);

                continue;
            }

            if (currentKey is not null && char.IsWhiteSpace(raw[0]))
                result[currentKey].Add(raw.Trim());
        }

        return result;
    }

    private static string? First(
        Dictionary<string, List<string>> fields,
        params string[] keys)
    {
        foreach (var key in keys)
            if (fields.TryGetValue(key, out var values) && values.Count > 0)
                return values[0];

        return null;
    }

    private static IReadOnlyList<string> Values(
        Dictionary<string, List<string>> fields,
        params string[] keys)
    {
        foreach (var key in keys)
            if (fields.TryGetValue(key, out var values))
                return values;

        return Array.Empty<string>();
    }

    private static int? ParseProblemCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var match = Regex.Match(value, @"^\s*(\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, out var parsed)
            ? parsed
            : null;
    }
}
