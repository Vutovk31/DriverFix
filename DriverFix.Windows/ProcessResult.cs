namespace DriverFix.Windows;

public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError
);
