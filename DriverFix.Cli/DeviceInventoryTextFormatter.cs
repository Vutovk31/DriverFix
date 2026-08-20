using System.Text;
using DriverFix.Core.Models;

namespace DriverFix.Cli;

public static class DeviceInventoryTextFormatter
{
    public static string Format(IReadOnlyList<DeviceInventoryItem> devices)
    {
        ArgumentNullException.ThrowIfNull(devices);

        var builder = new StringBuilder();
        builder.AppendLine($"Connected devices: {devices.Count}");

        for (var index = 0; index < devices.Count; index++)
        {
            var device = devices[index];

            builder.AppendLine();
            builder.AppendLine($"[{index + 1}] {Value(device.DeviceDescription)}");
            builder.AppendLine($"Instance ID: {device.InstanceId}");
            builder.AppendLine($"Class: {Value(device.ClassName)}");
            builder.AppendLine($"Manufacturer: {Value(device.Manufacturer)}");
            builder.AppendLine($"Status: {Value(device.Status)}");
            builder.AppendLine($"Problem Code: {(device.ProblemCode?.ToString() ?? "none")}");
            AppendList(builder, "Hardware IDs", device.HardwareIds);
            AppendList(builder, "Compatible IDs", device.CompatibleIds);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendList(
        StringBuilder builder,
        string label,
        IReadOnlyList<string> values)
    {
        builder.AppendLine($"{label}:");

        if (values.Count == 0)
        {
            builder.AppendLine("  - none");
            return;
        }

        foreach (var value in values)
            builder.AppendLine($"  - {value}");
    }

    private static string Value(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
}
