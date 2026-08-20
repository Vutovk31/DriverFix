using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text.Json;
using DriverFix.Core.Elevation;

namespace DriverFix.Windows.Elevation;

public sealed class ElevatedWorkerBroker
{
    public async Task<ElevatedResponse> ExecuteAsync(
        string elevatedWorkerPath,
        ElevatedOperation operation,
        string? infPath = null,
        string? instanceId = null,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Elevated worker broker requires Windows.");
        if (string.IsNullOrWhiteSpace(elevatedWorkerPath) || !File.Exists(elevatedWorkerPath))
            throw new FileNotFoundException("DriverFix.Elevated executable was not found.", elevatedWorkerPath);

        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var pipeName = $"DriverFix-{Guid.NewGuid():N}";

        using var pipe = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

        var start = new ProcessStartInfo
        {
            FileName = Path.GetFullPath(elevatedWorkerPath),
            UseShellExecute = true,
            Verb = "runas",
            Arguments = $"\"{pipeName}\" \"{nonce}\""
        };

        Process.Start(start) ?? throw new InvalidOperationException("Failed to start elevated worker.");
        await pipe.WaitForConnectionAsync(cancellationToken);

        var request = new ElevatedRequest(nonce, operation, infPath, instanceId);
        var validation = ElevatedOperationValidator.Validate(request);
        if (validation is not null)
            return new(false, null, false, validation);

        await JsonSerializer.SerializeAsync(pipe, request, cancellationToken: cancellationToken);
        await pipe.FlushAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<ElevatedResponse>(pipe, cancellationToken: cancellationToken)
            ?? new ElevatedResponse(false, null, false, "Elevated worker returned no response.");
    }
}
