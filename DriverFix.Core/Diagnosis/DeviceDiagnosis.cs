namespace DriverFix.Core.Diagnosis;

public sealed record DeviceDiagnosis(
    DiagnosisKind Kind,
    DiagnosisConfidence Confidence,
    string Evidence
);
