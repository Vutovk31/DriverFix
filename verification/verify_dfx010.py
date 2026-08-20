from pathlib import Path

root = Path(__file__).resolve().parents[1]
model = (root / "DriverFix.Core/Candidates/DriverUpdateCandidate.cs").read_text(encoding="utf-8")
provider = (root / "DriverFix.Windows/WindowsUpdateDriverCandidateProvider.cs").read_text(encoding="utf-8")
evaluator = (root / "DriverFix.Core/Services/DriverCandidateEligibilityEvaluator.cs").read_text(encoding="utf-8")
parser = (root / "DriverFix.Windows/DriverCandidateJsonParser.cs").read_text(encoding="utf-8")

checks = {
    "wua_read_only_search": "Microsoft.Update.Session" in provider and "CreateUpdateSearcher" in provider,
    "driver_search_criteria": "Type='Driver' and IsInstalled=0 and IsHidden=0" in provider,
    "candidate_identity": all(x in model for x in ["UpdateId", "Title", "DriverProvider", "DriverManufacturer", "DriverModel", "DriverClass", "DriverVerDate"]),
    "source_match_identifier_preserved": "SourceMatchIdentifier" in model,
    "ambiguous_identifier_is_compatible_evidence": "new(Array.Empty<string>(), new[] { SourceMatchIdentifier.Trim() })" in model,
    "eula_state_preserved": "EulaAccepted" in model and "EulaAccepted" in parser,
    "download_hidden_state_preserved": all(x in model for x in ["IsDownloaded", "IsHidden"]),
    "exact_match_required": "if (!match.IsMatch)" in evaluator,
    "eula_not_implicitly_accepted": "if (!candidate.EulaAccepted)" in evaluator and "will not accept or install it implicitly" in evaluator,
    "hidden_blocked": "if (candidate.IsHidden)" in evaluator,
    "bounded_stderr": "Limit(result.StandardError, 4096)" in provider,
    "no_mutation_or_download": all(token not in provider + evaluator for token in [
        ".AcceptEula", ".Download", ".Install", "IUpdateInstaller", "IUpdateDownloader",
        "/add-driver", "/delete-driver", "/remove-device", "/restart-device"
    ]),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)

if failed:
    raise SystemExit("DFX-010 CONTRACT FAIL: " + ", ".join(failed))

print(f"\nDFX-010 STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")
