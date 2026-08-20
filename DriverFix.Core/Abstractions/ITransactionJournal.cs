using DriverFix.Core.Transactions;

namespace DriverFix.Core.Abstractions;

public interface ITransactionJournal
{
    Task WriteAsync(TransactionJournalEntry entry, CancellationToken cancellationToken = default);
    Task<TransactionJournalEntry?> ReadAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransactionJournalEntry>> ReadIncompleteAsync(CancellationToken cancellationToken = default);
}
