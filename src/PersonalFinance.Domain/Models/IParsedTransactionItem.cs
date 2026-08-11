namespace PersonalFinance.Domain.Models;

public interface IParsedTransactionItem : ITrainingModelItem
{
    string? Amount { get; }
    string? Balance { get; }
    string? Date { get; }

    /// <summary>
    /// Stable identifier assigned by the upstream source. Null for CSV-sourced items,
    /// which have nothing durable to key on and fall back to composite matching.
    /// </summary>
    string? SourceId { get; }
}
