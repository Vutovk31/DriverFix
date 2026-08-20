using System.IO.Pipes;
using System.Text.Json;
using DriverFix.Core.Elevation;
using DriverFix.Windows;
using DriverFix.Windows.Elevation;

if (args.Length != 2 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
    return 64;

var pipeName = args[0];
var expectedNonce = args[1];

using var pipe = new NamedPipeClientStream(
    ".",
    pipeName,
    PipeDirection.InOut,
    PipeOptions.Asynchronous);
await pipe.ConnectAsync(30_000);

var request = await JsonSerializer.DeserializeAsync<ElevatedRequest>(pipe);
if (request is null || !string.Equals(request.Nonce, expectedNonce, StringComparison.Ordinal))
{
    await JsonSerializer.SerializeAsync(pipe, new ElevatedResponse(false, null, false, "IPC authentication failed."));
    await pipe.FlushAsync();
    return 65;
}

var validation = ElevatedOperationValidator.Validate(request);
if (validation is not null)
{
    await JsonSerializer.SerializeAsync(pipe, new ElevatedResponse(false, null, false, validation));
    await pipe.FlushAsync();
    return 66;
}

var runner = new ProcessRunner();
IReadOnlyList<string> commandArgs = request.Operation switch
{
    ElevatedOperation.InstallExactInf => new[] { "/add-driver", Path.GetFullPath(request.InfPath!), "/install" },
    ElevatedOperation.RestartExactDevice => new[] { "/restart-device", request.InstanceId! },
    ElevatedOperation.RestoreExactBackup => new[] { "/add-driver", Path.GetFullPath(request.InfPath!), "/install" },
    _ => throw new InvalidOperationException("Unsupported elevated operation.")
};

var result = await runner.RunAsync("pnputil.exe", commandArgs);
var rebootRequired = result.ExitCode is 3010 or 1641;
var response = new ElevatedResponse(
    result.ExitCode == 0 || rebootRequired,
    result.ExitCode,
    rebootRequired,
    result.ExitCode == 0
        ? "Elevated operation completed."
        : $"PnPUtil exited with code {result.ExitCode}.");

await JsonSerializer.SerializeAsync(pipe, response);
await pipe.FlushAsync();
return response.Accepted ? 0 : 1;
