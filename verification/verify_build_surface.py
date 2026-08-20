from pathlib import Path
import shutil
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
solution = root / "DriverFix.sln"
projects = [
    root / "DriverFix.Core/DriverFix.Core.csproj",
    root / "DriverFix.Persistence/DriverFix.Persistence.csproj",
    root / "DriverFix.Windows/DriverFix.Windows.csproj",
    root / "DriverFix.Cli/DriverFix.Cli.csproj",
    root / "DriverFix.Elevated/DriverFix.Elevated.csproj",
]

text = solution.read_text(encoding="utf-8")
checks = {
    "solution_present": solution.exists(),
    "all_projects_present": all(p.exists() for p in projects),
    "all_projects_in_solution": all(str(p.relative_to(root)).replace('/', '\\') in text for p in projects),
    "debug_configuration": "Debug|Any CPU" in text,
    "release_configuration": "Release|Any CPU" in text,
    "core_net10": "<TargetFramework>net10.0</TargetFramework>" in projects[0].read_text(encoding="utf-8"),
    "windows_targeting": all(
        "<TargetFramework>net10.0-windows</TargetFramework>" in p.read_text(encoding="utf-8")
        for p in [projects[2], projects[3], projects[4]]
    ),
    "elevated_manifest": "<ApplicationManifest>app.manifest</ApplicationManifest>" in projects[4].read_text(encoding="utf-8"),
}

failed = [name for name, ok in checks.items() if not ok]
for name, ok in checks.items():
    print(("PASS" if ok else "FAIL") + ": " + name)
if failed:
    raise SystemExit("BUILD SURFACE CONTRACT FAIL: " + ", ".join(failed))

print(f"\nBUILD SURFACE STATIC CONTRACT PASS: {len(checks)}/{len(checks)}")

dotnet = shutil.which("dotnet")
if dotnet is None:
    print("DOTNET BUILD: BLOCKED_ENVIRONMENT (dotnet SDK not found)")
    sys.exit(2)

completed = subprocess.run(
    [dotnet, "build", str(solution), "-c", "Release", "--nologo"],
    cwd=root,
    text=True,
)
sys.exit(completed.returncode)
