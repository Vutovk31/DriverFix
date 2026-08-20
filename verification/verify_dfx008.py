from pathlib import Path

root = Path(__file__).resolve().parents[1]
engine = (root / "DriverFix.Core/Diagnosis/DiagnosisEngine.cs").read_text(encoding="utf-8")
kind = (root / "DriverFix.Core/Diagnosis/DiagnosisKind.cs").read_text(encoding="utf-8")
confidence = (root / "DriverFix.Core/Diagnosis/DiagnosisConfidence.cs").read_text(encoding="utf-8")

checks = {
    "all_diagnosis_kinds": all(x in kind for x in [
        "DriverMissing", "DeviceProblem", "DriverMetadataMissing",
        "DriverUnsigned", "DriverVersionUnknown", "Healthy"
    ]),
    "confidence_levels": all(x in confidence for x in ["Medium", "High"]),
    "problem_28_is_missing": "problemCode == 28" in engine and "DiagnosisKind.DriverMissing" in engine,
    "other_problem_is_device_problem": "problemCode is > 0" in engine and "DiagnosisKind.DeviceProblem" in engine,
    "missing_metadata_not_missing_driver": "DiagnosisKind.DriverMetadataMissing" in engine and "not proof that the driver is missing" in engine,
    "unsigned_is_high": "driver.IsSigned == false" in engine and "DiagnosisKind.DriverUnsigned" in engine,
    "unknown_version_is_medium": "string.IsNullOrWhiteSpace(driver.DriverVersion)" in engine and "DiagnosisKind.DriverVersionUnknown" in engine,
    "healthy_requires_no_prior_failure": "DiagnosisKind.Healthy" in engine,
    "no_version_age_inference": all(token not in engine for token in ["latest", "outdated", "newer", "older"]),
    "no_driver_mutation": all(token not in engine for token in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-008 CONTRACT FAIL: " + ", ".join(failed))

print(f"\nDFX-008 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
