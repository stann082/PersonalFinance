using PersonalFinance.Domain.Models;

namespace PersonalFinance.Infrastructure.SimpleFin;

/// <summary>
/// Adapts a SimpleFIN transaction onto <see cref="IParsedTransactionItem"/> so it can flow
/// through the same categorize-and-save pipeline the CSV importer uses.
/// </summary>
public class SimpleFinTransactionItem : IParsedTransactionItem
{

    #region Constructors

    public SimpleFinTransactionItem(SimpleFinAccount account, SimpleFinTransaction transaction)
    {
        Amount = transaction.Amount;
        Category = null;
        Date = ToLocalDate(transaction).ToShortDateString();
        Description = FirstNonBlank(transaction.Description, transaction.Payee, transaction.Memo);
        IsRecurring = null;
        SourceId = $"{account.Id}:{transaction.Id}";
    }

    #endregion

    #region Properties

    public string? Amount { get; }

    /// <summary>
    /// SimpleFIN exposes no per-transaction running balance, only an account-level one,
    /// so there is nothing honest to put here.
    /// </summary>
    public string? Balance => null;

    public string? Category { get; set; }
    public string? Date { get; }
    public string? Description { get; set; }
    public bool? IsRecurring { get; set; }
    public string? SourceId { get; }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Collapses the epoch timestamp to local midnight so synced rows sort and range-filter
    /// alongside CSV rows, which carry no time component.
    /// </summary>
    private static DateTime ToLocalDate(SimpleFinTransaction transaction)
    {
        long epochSeconds = transaction.TransactedAt ?? transaction.Posted;
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds).ToLocalTime().Date;
    }

    private static string? FirstNonBlank(params string?[] candidates)
    {
        return candidates.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));
    }

    #endregion

}
