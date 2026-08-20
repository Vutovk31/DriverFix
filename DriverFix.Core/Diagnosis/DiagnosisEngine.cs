using DriverFix.Core.Models;

namespace DriverFix.Core.Diagnosis;

public static class DiagnosisEngine
{
    public static DeviceDiagnosis Diagnose(DeviceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var problemCode = snapshot.Device.ProblemCode;
        var driver = snapshot.InstalledDriver;

        if (problemCode == 28)
            return new(
                DiagnosisKind.DriverMissing,
                DiagnosisConfidence.High,
                "PnP Problem Code 28 explicitly indicates no driver is installed for the device.");

        if (problemCode is > 0)
            return new(
                DiagnosisKind.DeviceProblem,
                DiagnosisConfidence.High,
                $"PnP reports device problem code {problemCode}.");

        if (driver is null)
            return new(
                DiagnosisKind.DriverMetadataMissing,
                DiagnosisConfidence.Medium,
                "PnP reports no explicit device error, but installed driver metadata could not be joined. This is not proof that the driver is missing.");

        if (driver.IsSigned == false)
            return new(
                DiagnosisKind.DriverUnsigned,
                DiagnosisConfidence.High,
                "Installed driver metadata explicitly reports IsSigned=false.");

        if (string.IsNullOrWhiteSpace(driver.DriverVersion))
            return new(
                DiagnosisKind.DriverVersionUnknown,
                DiagnosisConfidence.Medium,
                "Installed driver metadata exists, but DriverVersion is absent.");

        return new(
            DiagnosisKind.Healthy,
            DiagnosisConfidence.High,
            "PnP reports no device problem and installed driver metadata is present with a known version.");
    }
}
