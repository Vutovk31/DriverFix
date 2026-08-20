using System.Text.Json;
using DriverFix.Core.Abstractions;
using DriverFix.Core.Transactions;

namespace DriverFix.Persistence;

public sealed class JsonTransactionJournal : ITransactionJournal
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _rootDirectory;

    public JsonTransactionJournal(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Journal root directory is required.", nameof(rootDirectory));

        _rootDirectory = Path.GetFullPath(rootDirectory);
        Directory.CreateDirectory(_rootDirectory);
    }

    public async Task WriteAsync(TransactionJournalEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

        var path = PathFor(entry.TransactionId);
        var temp = path + ".tmp";
        var payload = JsonSerializer.SerializeToUtf8Bytes(entry, JsonOptions);

        await using (var stream = new FileStream(
            temp,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            FileOptions.WriteThrough | FileOptions.Asynchronous))
        {
            await stream.WriteAsync(payload, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            stream.Flush(flushToDisk: true);
        }

        if (File.Exists(path))
            File.Replace(temp, path, null);
        else
            File.Move(temp, path);
    }

    public async Task<TransactionJournalEntry?> ReadAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var path = PathFor(transactionId);
        if (!File.Exists(path))
            return null;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<TransactionJournalEntry>(stream, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<TransactionJournalEntry>> ReadIncompleteAsync(CancellationToken cancellationToken = default)
    {
        var entries = new List<TransactionJournalEntry>();
        foreach (var path in Directory.EnumerateFiles(_rootDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var stream = File.OpenRead(path);
                var entry = await JsonSerializer.DeserializeAsync<TransactionJournalEntry>(stream, JsonOptions, cancellationToken);
                if (entry is null || entry.Phase is TransactionPhase.Verified or TransactionPhase.RolledBack)
                    continue;
                entries.Add(entry);
            }
            catch (JsonException)
            {
                // Corrupt entries are intentionally not auto-replayed. Hardening/quarantine is a later boundary.
            }
        }

        return entries.OrderBy(x => x.UpdatedUtc).ToArray();
    }

    private string PathFor(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId is required.", nameof(transactionId));

        var fileName = transactionId.Trim() + ".json";
        if (!string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
            throw new ArgumentException("TransactionId contains path characters.", nameof(transactionId));

        var full = Path.GetFullPath(Path.Combine(_rootDirectory, fileName));
        if (!full.StartsWith(_rootDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Transaction journal path escaped its configured root.");
        return full;
    }
}
