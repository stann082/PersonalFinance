using MongoDB.Bson;
using PersonalFinance.Domain.Misc;

namespace PersonalFinance.Domain.Models;

public class Transaction : ITrainingModelItem
{

    #region Constructors

    public Transaction()
    {
        // for unit tests
    }

    public Transaction(IParsedTransactionItem item)
    {
        InitializeProperties(item);
    }

    #endregion

    #region Properties

    public ObjectId Id { get; set; }
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public string? Category { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Identifier from the upstream source (SimpleFIN). Null for CSV-imported transactions.
    /// </summary>
    public string? ExternalId { get; set; }

    public bool IsDeposit => TransactionType == TransactionType.Credit;
    public bool? IsRecurring { get; set; }
    public string? TransactionId { get; set; }
    public TransactionType TransactionType { get; private set; }

    #endregion

    #region Overridden Methods

    public override string ToString()
    {
        return $"{Date.ToShortDateString()}: ${Amount} {Description}";
    }

    #endregion

    #region Helper Methods

    private void InitializeProperties(IParsedTransactionItem item)
    {
        TransactionId = Guid.NewGuid().ToString();
        Amount = decimal.TryParse(item.Amount, out var amountResult) ? amountResult : 0;
        Balance = decimal.TryParse(item.Balance, out var balanceResult) ? balanceResult : 0;
        Category = item.Category;
        Date = DateTime.TryParse(item.Date, out var dateResult) ? dateResult : DateTime.MinValue;
        Description = item.Description;
        ExternalId = item.SourceId;
        IsRecurring = Amount < 0 ? (item.IsRecurring ?? false) : null;
        TransactionType = Amount < 0 ? TransactionType.Debit : TransactionType.Credit;
    }

    #endregion

}
