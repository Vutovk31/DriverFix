from pathlib import Path

root = Path(__file__).resolve().parents[1]
matcher = (root / "DriverFix.Core/Matching/DriverIdentifierMatcher.cs").read_text(encoding="utf-8")
kind = (root / "DriverFix.Core/Matching/DriverIdentifierMatchKind.cs").read_text(encoding="utf-8")
result = (root / "DriverFix.Core/Matching/DriverIdentifierMatch.cs").read_text(encoding="utf-8")

checks = {
    "four_match_kinds": all(x in kind for x in [
        "HardwareToHardware", "HardwareToCompatible",
        "CompatibleToHardware", "CompatibleToCompatible"
    ]),
    "tier_scores_present": all(x in matcher for x in ["4000", "3000", "2000", "1000"]),
    "exact_case_insensitive": "StringComparison.OrdinalIgnoreCase" in matcher,
    "trim_only_normalization": "value.Trim()" in matcher,
    "position_penalty": "deviceIndex + candidateIndex" in matcher,
    "no_match_zero": "DriverIdentifierMatchKind.None" in matcher and "0," in matcher,
    "result_has_is_match": "bool IsMatch" in result,
    "no_substring_matching": all(x not in matcher for x in ["Contains(", "StartsWith(", "EndsWith("]),
    "no_ven_dev_parsing": all(x not in matcher for x in ["VEN_", "DEV_", "SUBSYS_", "ClassName", "Manufacturer"]),
    "no_driver_mutation": all(x not in matcher for x in [
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device", "/install"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-009 CONTRACT FAIL: " + ", ".join(failed))

# Executable reference behavior for opaque exact IDs.
def norm(value):
    return value.strip().lower()

def first_exact(device_ids, candidate_ids, base):
    for di, d in enumerate(device_ids):
        for ci, c in enumerate(candidate_ids):
            if d.strip() and c.strip() and norm(d) == norm(c):
                return max(1, base - di - ci)
    return 0

assert first_exact([" PCI\\VEN_A&DEV_B "], ["pci\\ven_a&dev_b"], 4000) == 4000
assert first_exact(["ABC"], ["ABC&REV_1"], 4000) == 0
assert first_exact(["X", "Y"], ["Z", "Y"], 4000) == 3998
print("PASS: exact_identifier_reference_behavior")

print(f"\nDFX-009 STATIC/REFERENCE CONTRACT PASS: {len(checks) + 1}/{len(checks) + 1}")
