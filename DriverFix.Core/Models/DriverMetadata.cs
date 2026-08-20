namespace DriverFix.Core.Models;

public sealed record DriverMetadata(
    string DeviceId,
    string? DeviceName,
    string? DriverProviderName,
    string? DriverVersion,
    string? DriverDate,
    string? InfName,
    bool? IsSigned,
    string? Signer
);
